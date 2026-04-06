using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.RegionPerformance;

/// <summary>
/// Region performance projection. Tracks per-region metrics.
/// Event-driven ONLY via Kafka. NO domain/runtime/engine imports.
/// Idempotent: version-skip for already-processed events.
/// </summary>
public sealed class RegionPerformanceProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.region-performance";

    public string[] EventTypes =>
    [
        "whyce.global.region.metrics-reported",
        "whyce.global.region.latency-updated",
        "whyce.global.region.status-changed"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRegionPerformanceViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();

        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);

        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var regionId = @event.Headers.TryGetValue("x-source-region", out var r) ? r : "unknown";

        var model = existing ?? new RegionPerformanceReadModel
        {
            Id = aggregateId,
            RegionId = regionId,
            CommandsProcessed = 0,
            EventsPublished = 0,
            AvgCommandLatencyMs = 0,
            AvgReplicationLagMs = 0,
            ActiveConnections = 0,
            IsHealthy = true,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model with
        {
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        }, ct);

        _versionTracker.MarkProcessed(aggregateId, @event.Version);
    }

    public void ResetForRebuild() => _versionTracker.Reset();
}
