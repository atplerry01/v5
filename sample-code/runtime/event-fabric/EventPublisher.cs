using Whycespace.Runtime.EventFabric.Outbox;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Outbox-first event publisher. Events are always persisted to the outbox store
/// before being routed locally. Downstream transport (e.g. Kafka) is handled
/// asynchronously by the outbox publisher worker — never inline.
///
/// Partition key is derived from the event's CorrelationId (= AggregateId)
/// to guarantee ordering per aggregate on Kafka.
/// </summary>
public sealed class EventPublisher : IEventPublisher
{
    private readonly IOutboxStore _outboxStore;
    private readonly EventRouter _router;

    public EventPublisher(IOutboxStore outboxStore, EventRouter router)
    {
        ArgumentNullException.ThrowIfNull(outboxStore);
        ArgumentNullException.ThrowIfNull(router);
        _outboxStore = outboxStore;
        _router = router;
    }

    public async Task PublishAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var partitionKey = ResolvePartitionKey(@event);

        // 1. Persist to outbox first — this is the transactional boundary.
        await _outboxStore.AppendAsync(@event, partitionKey, cancellationToken);

        // 2. Route locally for in-process consumers (projections, event store).
        await _router.RouteAsync(@event, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<RuntimeEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        var eventList = events.ToList();

        foreach (var @event in eventList)
        {
            var partitionKey = ResolvePartitionKey(@event);

            // 1. Persist to outbox atomically per event.
            await _outboxStore.AppendAsync(@event, partitionKey, cancellationToken);
        }

        // 2. Route locally.
        foreach (var @event in eventList)
        {
            await _router.RouteAsync(@event, cancellationToken);
        }
    }

    /// <summary>
    /// Resolves partition key from event headers or correlation ID.
    /// Priority: x-aggregate-id header → CorrelationId.
    /// </summary>
    private static string ResolvePartitionKey(RuntimeEvent @event)
    {
        if (@event.Headers.TryGetValue("x-aggregate-id", out var aggregateId)
            && !string.IsNullOrWhiteSpace(aggregateId))
        {
            return aggregateId;
        }

        return @event.CorrelationId;
    }
}
