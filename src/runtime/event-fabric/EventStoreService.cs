using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Event Store Service — handles event persistence through the infrastructure abstraction.
/// The EventStore is the source of truth for aggregate state.
/// Only invoked by the Event Fabric orchestrator.
/// </summary>
public sealed class EventStoreService
{
    private readonly IEventStore _eventStore;

    public EventStoreService(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): forwards the
    // request/host-shutdown CancellationToken from EventFabric down to
    // IEventStore so the underlying Postgres adapter Execute*Async
    // calls honor cancellation.
    public async Task AppendAsync(
        Guid aggregateId,
        IReadOnlyList<EventEnvelope> envelopes,
        int expectedVersion = -1,
        CancellationToken cancellationToken = default)
    {
        if (envelopes.Count == 0) return;
        await _eventStore.AppendEventsAsync(aggregateId, envelopes, expectedVersion, cancellationToken);
    }

    public async Task<IReadOnlyList<object>> LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        return await _eventStore.LoadEventsAsync(aggregateId, cancellationToken);
    }
}
