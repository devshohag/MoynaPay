using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MoynaPay.Domain.Common;
using MoynaPay.Domain.Merchants;
using MoynaPay.Domain.Orders;
using MoynaPay.Domain.Payments;

namespace MoynaPay.Infrastructure.Persistence;

/// <summary>
/// The database.
///
/// Four schemas, one per module, so a table's name says which part of the product owns
/// it: merchants, orders, payments, voice. That is the same shape YoPay used and the
/// reason a person can read a query plan and know what they are looking at.
///
/// Two things here are defences rather than conveniences, and both are applied to every
/// entity by reflection rather than one at a time. Applying them by hand would work until
/// the day somebody adds an entity and forgets, which is exactly the day it matters.
/// </summary>
public sealed class MoynaPayDbContext(
    DbContextOptions<MoynaPayDbContext> options, ITenantContext tenant) : DbContext(options)
{
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<ApiCredential> ApiCredentials => Set<ApiCredential>();
    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderEvent> OrderEvents => Set<OrderEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<RawEvent> RawEvents => Set<RawEvent>();
    public DbSet<ParsedTransaction> ParsedTransactions => Set<ParsedTransaction>();
    public DbSet<PaymentClaim> PaymentClaims => Set<PaymentClaim>();
    public DbSet<PaymentMatch> PaymentMatches => Set<PaymentMatch>();
    public DbSet<PaymentSession> PaymentSessions => Set<PaymentSession>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DevicePairingToken> DevicePairingTokens => Set<DevicePairingToken>();
    public DbSet<ParserTemplate> ParserTemplates => Set<ParserTemplate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasDefaultSchema("moynapay");

        builder.ApplyConfigurationsFromAssembly(typeof(MoynaPayDbContext).Assembly);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var clr = entity.ClrType;

            if (!typeof(BaseEntity).IsAssignableFrom(clr)) continue;

            // MerchantId is the same column as TenantId under the name the payment code
            // uses. Mapping it would produce a second column that drifts from the first.
            builder.Entity(clr).Ignore(nameof(BaseEntity.MerchantId));

            // Soft delete plus merchant isolation, on every entity, always.
            //
            // A null current merchant means "no merchant in particular" and passes the
            // filter: the outbox worker drains every merchant's queue, and filtering it
            // to one would leave the rest undelivered. Isolation on the request path
            // comes from the API setting the context before anything is read.
            builder.Entity(clr).HasQueryFilter(BuildFilter(clr));

            // Optimistic concurrency, on every entity, always. EF puts the value it read
            // into the WHERE clause of the update, so an update built on a stale read
            // matches no row and raises rather than overwriting a decision it never saw.
            builder.Entity(clr).Property(nameof(BaseEntity.RowVersion)).IsConcurrencyToken();

            // Postgres keeps timestamptz as an instant and stores no offset, so Npgsql
            // refuses any DateTimeOffset whose offset is not zero rather than discarding
            // it silently. Everything upstream normalises to UTC; this says so once.
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset)
                    || property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetColumnType("timestamptz");
                }
            }
        }

        // snake_case, because every other tool that will ever touch this database - psql,
        // a backup, somebody's ad-hoc query - is easier to read that way, and because
        // unquoted mixed case in Postgres is a trap that only shows up in hand-written SQL.
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(SnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(SnakeCase(property.GetColumnName()));
            }

            foreach (var key in entity.GetKeys()) key.SetName(SnakeCase(key.GetName()!));
            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(SnakeCase(index.GetDatabaseName()!));
        }
    }

    /// <summary>
    /// e =&gt; !e.IsDeleted &amp;&amp; (tenant.Current == null || e.TenantId == tenant.Current)
    ///
    /// Built as an expression tree because the filter has to be attached per entity type
    /// and C# generics cannot express "whatever this runtime type is".
    /// </summary>
    private LambdaExpression BuildFilter(Type clr)
    {
        var parameter = Expression.Parameter(clr, "e");

        var notDeleted = Expression.Not(
            Expression.Property(parameter, nameof(BaseEntity.IsDeleted)));

        // Captured as a member access on the context, not a constant: EF compiles the
        // filter once and re-evaluates this per query, which is what makes the value
        // current rather than whatever it was at start-up.
        var current = Expression.Property(
            Expression.Property(Expression.Constant(this), nameof(Tenant)),
            nameof(Nullable<Guid>.HasValue));

        var currentValue = Expression.Property(
            Expression.Property(Expression.Constant(this), nameof(Tenant)),
            nameof(Nullable<Guid>.Value));

        var sameMerchant = Expression.Equal(
            Expression.Property(parameter, nameof(BaseEntity.TenantId)), currentValue);

        var tenantOk = Expression.OrElse(Expression.Not(current), sameMerchant);

        return Expression.Lambda(Expression.AndAlso(notDeleted, tenantOk), parameter);
    }

    /// <summary>
    /// Read by the query filter through an expression tree, so it cannot be private:
    /// the filter is compiled by EF, not invoked by this class.
    /// </summary>
    public Guid? Tenant => tenant.Current;

    public override int SaveChanges()
    {
        Stamp();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        Stamp();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// The audit trail and the concurrency token, written here rather than by each
    /// caller, because the one place a caller forgets is the one that matters.
    ///
    /// The bump happens before EF builds the statement, so the update reads
    /// SET row_version = new WHERE row_version = old. Callers that reach the database by
    /// raw SQL are outside this and have to bump it themselves.
    /// </summary>
    private void Stamp()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State != EntityState.Modified) continue;

            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            entry.Entity.RowVersion++;
        }
    }

    private static string SnakeCase(string name) =>
        Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
}
