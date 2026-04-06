using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.SystemHealth;

/// <summary>
/// Global system health projection. Aggregates health status from all regions.
/// Event-driven ONLY via Kafka. NO domain/runtime/engine imports.
/// Idempotent: version-skip for already-processed events.
/// </summary>
public sealed class GlobalSystemHealthProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.system-health";

    public string[] EventTypes =>
    [
        "whyce.global.health.reported",
        "whyce.global.health.degraded",
        "whyce.global.health.recovered"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGlobalSystemHealthViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();

        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);

        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var regionId = @event.Headers.TryGetValue("x-source-region", out var r) ? r : "unknown";

        var model = existing ?? new GlobalSystemHealthReadModel
        {
            Id = aggregateId,
            RegionId = regionId,
            SystemName = @event.AggregateType,
            Status = "Unknown",
            IsHealthy = true,
            ActiveAggregates = 0,
            AvgLatencyMs = 0,
            EventsProcessed = 0,
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
