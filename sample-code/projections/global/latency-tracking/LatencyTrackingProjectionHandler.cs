using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.LatencyTracking;

/// <summary>
/// Live latency tracking projection. Consumes command execution metrics
/// to maintain per-region, per-command-type latency percentiles.
/// Event-driven ONLY. Idempotent, version-skip.
/// </summary>
public sealed class LatencyTrackingProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.latency-tracking";

    public string[] EventTypes =>
    [
        "whyce.runtime.command.completed",
        "whyce.runtime.command.latency-recorded"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILatencyTrackingViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();
        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);
        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var regionId = @event.Headers.TryGetValue("x-source-region", out var r) ? r : "unknown";

        var model = existing ?? new LatencyTrackingReadModel
        {
            Id = aggregateId,
            RegionId = regionId,
            CommandType = @event.EventType,
            P50LatencyMs = 0,
            P95LatencyMs = 0,
            P99LatencyMs = 0,
            SampleCount = 0,
            IsWithinBudget = true,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model with
        {
            SampleCount = model.SampleCount + 1,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        }, ct);

        _versionTracker.MarkProcessed(aggregateId, @event.Version);
    }

    public void ResetForRebuild() => _versionTracker.Reset();
}
