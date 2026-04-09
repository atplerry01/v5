using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whyce.Runtime.EventFabric;

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
        IReadOnlyList<object> domainEvents,
        int expectedVersion = -1,
        CancellationToken cancellationToken = default)
    {
        if (domainEvents.Count == 0) return;
        await _eventStore.AppendEventsAsync(aggregateId, domainEvents, expectedVersion, cancellationToken);
    }

    public async Task<IReadOnlyList<object>> LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        return await _eventStore.LoadEventsAsync(aggregateId, cancellationToken);
    }
}
