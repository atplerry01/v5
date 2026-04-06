using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Base handler for economic projections with:
///   1. EventId-based deduplication — ignore duplicates
///   2. Global ordering guard — skip out-of-order events
///   3. Version tracking — Timestamp + Version tiebreaker
///   4. Replay-safe — same input stream always produces identical state
///
/// Subclasses implement ApplyAsync with domain-specific projection logic.
/// </summary>
public abstract class IdempotentEconomicProjectionHandler
{
    private const string ProcessedEventsProjection = "economic.processed-event-ids";

    protected readonly IClock _clock;

    protected IdempotentEconomicProjectionHandler(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public abstract string ProjectionName { get; }
    public abstract string[] EventTypes { get; }

    /// <summary>
    /// Wraps the handler with EventId deduplication and ordering guard.
    /// </summary>
    public async Task HandleAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(store);

        // 1. EventId deduplication — ignore duplicates
        var eventIdKey = $"{ProjectionName}:{@event.EventId}";
        var existing = await store.GetAsync<ProcessedEventMarker>(ProcessedEventsProjection, eventIdKey, cancellationToken);

        if (existing is not null)
            return;

        // 2. Apply projection logic (ordering guard enforced by subclass)
        await ApplyAsync(@event, store, cancellationToken);

        // 3. Mark EventId as processed with version tracking
        await store.SetAsync(ProcessedEventsProjection, eventIdKey,
            new ProcessedEventMarker
            {
                EventId = @event.EventId,
                Timestamp = @event.Timestamp,
                Version = @event.Version,
                ProcessedAt = _clock.UtcNowOffset
            },
            cancellationToken);
    }

    /// <summary>
    /// Builds a ProjectionRegistration for use with ProjectionEngine.
    /// </summary>
    public ProjectionRegistration ToRegistration()
    {
        return new ProjectionRegistration
        {
            ProjectionName = ProjectionName,
            EventTypes = EventTypes,
            Handler = HandleAsync
        };
    }

    /// <summary>
    /// Apply the projection. Guaranteed to be called at most once per EventId.
    /// Implement domain-specific read model updates here.
    /// Subclass MUST enforce ordering via ShouldSkipEvent before mutating state.
    /// </summary>
    protected abstract Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken);

    /// <summary>
    /// Global ordering guard. Returns true if the event should be skipped.
    ///   - event.Timestamp &lt; existing.LastEventTimestamp → SKIP
    ///   - event.Timestamp == existing.LastEventTimestamp AND event.Version &lt;= existing.LastEventVersion → SKIP
    ///   - else → APPLY
    /// </summary>
    protected static bool ShouldSkipEvent(
        DateTimeOffset eventTimestamp, long eventVersion,
        DateTimeOffset lastEventTimestamp, long lastEventVersion)
    {
        if (eventTimestamp < lastEventTimestamp)
            return true;

        if (eventTimestamp == lastEventTimestamp && eventVersion <= lastEventVersion)
            return true;

        return false;
    }
}

internal sealed class ProcessedEventMarker
{
    public Guid EventId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public long Version { get; init; }
    public DateTimeOffset ProcessedAt { get; init; }
}
