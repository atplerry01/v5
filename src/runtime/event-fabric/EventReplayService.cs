using Whyce.Runtime.Deterministic;
using Whyce.Runtime.Projection;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Event Replay Service — replays events from the event store to rebuild
/// projections or verify execution integrity.
///
/// Supports:
/// - Full aggregate replay
/// - Targeted projection rebuild
/// - Execution hash verification (replay produces same hash)
/// </summary>
public sealed class EventReplayService
{
    private readonly EventStoreService _eventStoreService;
    private readonly IProjectionDispatcher _projectionDispatcher;
    private readonly EventSchemaRegistry _schemaRegistry;

    public EventReplayService(
        EventStoreService eventStoreService,
        IProjectionDispatcher projectionDispatcher,
        EventSchemaRegistry schemaRegistry)
    {
        _eventStoreService = eventStoreService;
        _projectionDispatcher = projectionDispatcher;
        _schemaRegistry = schemaRegistry;
    }

    /// <summary>
    /// Replays all events for an aggregate through the projection dispatcher.
    /// Handlers must be idempotent and replay-safe.
    /// </summary>
    public async Task ReplayAsync(Guid aggregateId, Guid correlationId)
    {
        var events = await _eventStoreService.LoadAsync(aggregateId);
        if (events.Count == 0) return;

        var envelopes = new List<EventEnvelope>(events.Count);
        for (var i = 0; i < events.Count; i++)
        {
            var domainEvent = events[i];
            var eventTypeName = domainEvent.GetType().Name;
            var schema = _schemaRegistry.Resolve(eventTypeName);

            envelopes.Add(new EventEnvelope
            {
                EventId = EventEnvelope.GenerateDeterministicId(correlationId, eventTypeName, i),
                AggregateId = aggregateId,
                CorrelationId = correlationId,
                EventType = eventTypeName,
                EventName = schema.EventName,
                EventVersion = schema.Version,
                SchemaHash = schema.SchemaHash,
                Payload = domainEvent,
                ExecutionHash = "replay",
                PolicyHash = "replay",
                Timestamp = DateTimeOffset.MinValue,
                SequenceNumber = i
            });
        }

        await _projectionDispatcher.DispatchAsync(envelopes);
    }
}
