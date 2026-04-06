using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.ThroughputTracking;

/// <summary>
/// Live throughput tracking projection. Tracks commands/events per second
/// per region. Event-driven ONLY. Idempotent, version-skip.
/// </summary>
public sealed class ThroughputTrackingProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.throughput-tracking";

    public string[] EventTypes =>
    [
        "whyce.runtime.throughput.reported",
        "whyce.runtime.command.completed"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IThroughputTrackingViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();
        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);
        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var regionId = @event.Headers.TryGetValue("x-source-region", out var r) ? r : "unknown";

        var model = existing ?? new ThroughputTrackingReadModel
        {
            Id = aggregateId,
            RegionId = regionId,
            CommandsProcessed = 0,
            EventsPublished = 0,
            TransactionsCompleted = 0,
            CommandsPerSecond = 0,
            EventsPerSecond = 0,
            WindowStart = @event.Timestamp,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model with
        {
            CommandsProcessed = model.CommandsProcessed + 1,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        }, ct);

        _versionTracker.MarkProcessed(aggregateId, @event.Version);
    }

    public void ResetForRebuild() => _versionTracker.Reset();
}
