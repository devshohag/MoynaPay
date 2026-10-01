namespace MoynaPay.Infrastructure;

/// <summary>
/// Which merchant the current unit of work belongs to, when it belongs to one.
///
/// Set by the API once a signed request has been attributed to a merchant, and read by
/// the database's global query filter. Every store method already takes a merchant id, so
/// this is not how isolation is achieved - it is what catches the query that forgets.
///
/// Null is a legitimate value and means "no merchant in particular". The outbox worker
/// drains every merchant's queue; filtering it to one would leave the rest undelivered.
/// The difference matters enough to be a property rather than a convention: code that
/// crosses merchants should have to say so.
/// </summary>
public interface ITenantContext
{
    Guid? Current { get; }

    void Set(Guid merchantId);
}

/// <summary>
/// One per request in the API, one per scope in a worker.
///
/// Deliberately write-once. A context that can be reassigned mid-request is one stray
/// call away from the rest of that request reading somebody else's rows.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    public Guid? Current { get; private set; }

    public void Set(Guid merchantId)
    {
        if (Current is { } already && already != merchantId)
        {
            throw new InvalidOperationException(
                "The merchant for this scope is already set. Reassigning it mid-request " +
                "would let the rest of the work read another merchant's rows.");
        }

        Current = merchantId;
    }
}
