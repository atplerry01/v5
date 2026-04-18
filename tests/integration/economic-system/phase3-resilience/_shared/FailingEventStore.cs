using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

/// <summary>
/// Test double that injects a controlled persistence failure into the
/// event-store append path. Lets Phase 3 failure tests prove:
///
///   * a simulated persistence failure propagates as an exception,
///   * the idempotency middleware releases its claim on the failure
///     branch so a retry of the same logical command can proceed,
///   * a successful retry after the failure produces a single
///     persisted event (no duplicate), preserving ledger invariants.
///
/// The double delegates every call to an inner <see cref="InMemoryEventStore"/>
/// so all other harness assertions (versions, cross-aggregate isolation)
/// still hold.
/// </summary>
public sealed class FailingEventStore : Whycespace.Shared.Contracts.Infrastructure.Persistence.IEventStore
{
    private readonly InMemoryEventStore _inner;
    private int _remainingFailures;
    private readonly object _lock = new();

    public FailingEventStore(InMemoryEventStore inner, int failuresToInject)
    {
        _inner = inner;
        _remainingFailures = failuresToInject;
    }

    public int FailuresRemaining
    {
        get { lock (_lock) return _remainingFailures; }
    }

    public Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        => _inner.LoadEventsAsync(aggregateId, cancellationToken);

    public Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<IEventEnvelope> envelopes, int expectedVersion, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_remainingFailures > 0)
            {
                _remainingFailures--;
                throw new InvalidOperationException(
                    $"FailingEventStore: injected persistence failure for aggregate {aggregateId} (remaining={_remainingFailures})");
            }
        }
        return _inner.AppendEventsAsync(aggregateId, envelopes, expectedVersion, cancellationToken);
    }

    // Test-only convenience proxies.
    public IReadOnlyList<int> Versions(Guid aggregateId) => _inner.Versions(aggregateId);
    public IReadOnlyList<object> AllEvents(Guid aggregateId) => _inner.AllEvents(aggregateId);
    public InMemoryEventStore Inner => _inner;
}
