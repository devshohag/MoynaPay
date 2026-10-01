using Microsoft.EntityFrameworkCore;
using MoynaPay.Application.Abstractions;
using MoynaPay.Domain.Merchants;
using MoynaPay.Domain.Orders;

namespace MoynaPay.Infrastructure.Persistence;

/// <summary>
/// The same ports as the in-memory stores, against Postgres.
///
/// Every method still takes a merchant id even though a global query filter is in place.
/// Belt and braces on purpose: the filter protects a query somebody forgot to scope, and
/// the parameter protects against a context whose merchant was never set - a worker, say,
/// reaching into one merchant's data.
/// </summary>
public sealed class EfMerchantStore(MoynaPayDbContext db) : IMerchantStore
{
    public Task<Merchant?> FindAsync(Guid merchantId, CancellationToken ct = default) =>
        db.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId, ct);

    public async Task<Subscription> SubscriptionAsync(Guid merchantId, CancellationToken ct = default)
    {
        var found = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.TenantId == merchantId, ct).ConfigureAwait(false);

        // A merchant with no row has bought nothing. An empty subscription keeps the
        // pipeline's answer defined - skip every step - rather than throwing somewhere
        // deep in a worker.
        return found ?? new Subscription { TenantId = merchantId };
    }

    /// <summary>
    /// Resolves a signing key before any merchant is known, so this one query is
    /// deliberately outside the tenant filter. It is the only one.
    /// </summary>
    public Task<ApiCredential?> FindCredentialAsync(string keyId, CancellationToken ct = default) =>
        db.ApiCredentials
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.KeyId == keyId && !c.IsDeleted && c.RevokedAt == null, ct);

    public Task<WebhookEndpoint?> WebhookAsync(Guid merchantId, CancellationToken ct = default) =>
        db.WebhookEndpoints.FirstOrDefaultAsync(w => w.TenantId == merchantId, ct);
}

public sealed class EfOrderStore(MoynaPayDbContext db) : IOrderStore
{
    public Task<Order?> FindByReferenceAsync(Guid merchantId, string reference, CancellationToken ct = default) =>
        db.Orders.FirstOrDefaultAsync(o => o.TenantId == merchantId && o.Reference == reference, ct);

    public Task<Order?> FindByIdAsync(Guid merchantId, Guid orderId, CancellationToken ct = default) =>
        db.Orders.FirstOrDefaultAsync(o => o.TenantId == merchantId && o.Id == orderId, ct);

    /// <summary>
    /// The order and the event that explains it, in one transaction.
    ///
    /// A unique violation here is not a fault: it is two requests for the same reference
    /// arriving at once, which is exactly what the index is for. The caller looks the
    /// existing order up and answers with it, the same as if it had lost the race by a
    /// second instead of a millisecond.
    /// </summary>
    public async Task SaveNewAsync(Order order, OrderEvent first, CancellationToken ct = default)
    {
        db.Orders.Add(order);
        db.OrderEvents.Add(first);

        try
        {
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException("duplicate reference", ex);
        }
    }

    public async Task SaveChangeAsync(Order order, OrderEvent change, OutboxMessage? notify,
        CancellationToken ct = default)
    {
        db.OrderEvents.Add(change);

        // In the same transaction as the change, always. A notification that exists
        // without the change that caused it, or a change without the notification, is a
        // state nothing downstream can make sense of.
        if (notify is not null) db.OutboxMessages.Add(notify);

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Order>> ListAsync(Guid merchantId, OrderQuery query,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = db.Orders.AsNoTracking().Where(o => o.TenantId == merchantId);

        if (query.NeedsHumanOnly) rows = rows.Where(o => o.Status == OrderStatus.NeedsHuman);
        else if (query.Status is { } status) rows = rows.Where(o => o.Status == status);

        if (query.Before is { } before) rows = rows.Where(o => o.CreatedAt < before);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var needle = query.Search.Trim();

            rows = rows.Where(o =>
                EF.Functions.ILike(o.Reference, $"%{needle}%")
                || EF.Functions.ILike(o.CustomerName, $"%{needle}%")
                || o.Msisdn.Contains(needle));
        }

        return await rows
            .OrderByDescending(o => o.CreatedAt)
            .Take(Math.Clamp(query.Take, 1, 200))
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OrderEvent>> TimelineAsync(Guid merchantId, Guid orderId,
        CancellationToken ct = default) =>
        await db.OrderEvents.AsNoTracking()
            .Where(e => e.TenantId == merchantId && e.OrderId == orderId)
            .OrderBy(e => e.At)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <summary>
    /// 23505 is Postgres' unique_violation. Matched on the code rather than the message,
    /// which is localised and has changed between server versions.
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is Npgsql.PostgresException { SqlState: "23505" };
}

/// <summary>
/// Nonces, in the database rather than in one process's memory.
///
/// Memory was enough while there was one API. With two behind a load balancer, a captured
/// request replayed against the other instance would be accepted, because that instance
/// never saw the first one.
///
/// Rows older than the signature tolerance are already refused by the clock, so a sweep
/// can delete them; keeping them would grow the table forever for no benefit.
/// </summary>
public sealed class EfNonceStore(MoynaPayDbContext db) : INonceStore
{
    public async Task<bool> TryUseAsync(string keyId, string nonce, CancellationToken ct = default)
    {
        var id = Guid.CreateVersion7();
        var expires = DateTimeOffset.UtcNow.AddSeconds(
            Application.Security.RequestSignature.ToleranceSeconds);

        // ON CONFLICT DO NOTHING: the insert either takes the nonce or reports that
        // somebody already has it. Checking first and then inserting would leave a window
        // between the two in which a replay slips through.
        //
        // Every NOT NULL column is listed. A raw insert skips the conventions EF applies
        // on SaveChanges, so anything left out here fails at run time, not at build time,
        // and the failure looks like every signed request suddenly being refused.
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO merchants.request_nonces
                 (id, tenant_id, key_id, nonce, seen_at, expires_at,
                  created_at, updated_at, is_deleted, row_version)
             VALUES
                 ({id}, {Guid.Empty}, {keyId}, {nonce}, now(), {expires},
                  now(), now(), false, 0)
             ON CONFLICT (key_id, nonce) DO NOTHING
             """, ct).ConfigureAwait(false);

        return inserted == 1;
    }
}
