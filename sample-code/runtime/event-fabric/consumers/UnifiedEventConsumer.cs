namespace Whycespace.Runtime.EventFabric.Consumers;

/// <summary>
/// Single unified event consumer that handles all incoming events
/// with idempotency-first semantics:
///   1. Check if event was already processed (idempotency guard)
///   2. Route to all registered consumers via EventRouter
///   3. Mark event as processed
///
/// Replaces any per-concern consumer duplication (KafkaEventConsumer,
/// IdempotentEventConsumer, etc.) with a single consumption path.
/// </summary>
public sealed class UnifiedEventConsumer
{
    private readonly IEventIdempotencyStore _idempotencyStore;
    private readonly EventRouter _router;

    public UnifiedEventConsumer(IEventIdempotencyStore idempotencyStore, EventRouter router)
    {
        ArgumentNullException.ThrowIfNull(idempotencyStore);
        ArgumentNullException.ThrowIfNull(router);
        _idempotencyStore = idempotencyStore;
        _router = router;
    }

    public async Task ConsumeAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // 1. Idempotency guard — skip if already processed.
        if (await _idempotencyStore.ExistsAsync(@event.EventId, cancellationToken))
            return;

        // 2. Route to all registered consumers (persistence, projections, etc.).
        await _router.RouteAsync(@event, cancellationToken);

        // 3. Mark processed — after successful routing.
        await _idempotencyStore.MarkProcessedAsync(@event.EventId, cancellationToken);
    }

    public async Task ConsumeAsync(IEnumerable<RuntimeEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        foreach (var @event in events)
        {
            await ConsumeAsync(@event, cancellationToken);
        }
    }
}
