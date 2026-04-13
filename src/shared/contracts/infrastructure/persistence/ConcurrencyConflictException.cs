namespace Whycespace.Shared.Contracts.Infrastructure.Persistence;

/// <summary>
/// Thrown by <see cref="IEventStore.AppendEventsAsync"/> when the caller's
/// asserted <c>expectedVersion</c> does not match the aggregate's actual
/// stored version at the moment of write. This is the named, recognizable
/// failure mode of optimistic concurrency control (phase1-gate-H8b).
///
/// Carries both the expected and actual version so callers can decide
/// whether to surface the conflict (e.g. as HTTP 409), reload + re-execute,
/// or escalate. The library NEVER retries on its own ($12 — no partial
/// completion, no implicit recovery).
///
/// Sentinel: a caller passing <c>expectedVersion = -1</c> opts out of the
/// check. The exception is therefore only raised when the caller has
/// explicitly asserted a version and the assertion is wrong.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public Guid AggregateId { get; }
    public int ExpectedVersion { get; }
    public int ActualVersion { get; }

    public ConcurrencyConflictException(Guid aggregateId, int expectedVersion, int actualVersion)
        : base($"Concurrent append conflict on aggregate {aggregateId}: " +
               $"expected version {expectedVersion}, found {actualVersion}.")
    {
        AggregateId = aggregateId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
