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

    public async Task AppendAsync(Guid aggregateId, IReadOnlyList<object> domainEvents, int expectedVersion = -1)
    {
        if (domainEvents.Count == 0) return;
        await _eventStore.AppendEventsAsync(aggregateId, domainEvents, expectedVersion);
    }

    public async Task<IReadOnlyList<object>> LoadAsync(Guid aggregateId)
    {
        return await _eventStore.LoadEventsAsync(aggregateId);
    }
}
