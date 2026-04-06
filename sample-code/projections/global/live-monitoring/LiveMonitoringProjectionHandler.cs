using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.LiveMonitoring;

/// <summary>
/// Real-time live monitoring projection. Consumes region activation,
/// anomaly, and performance events. Event-driven ONLY via Kafka.
/// NO domain/runtime/engine imports. Idempotent.
/// </summary>
public sealed class LiveMonitoringProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.live-monitoring";

    public string[] EventTypes =>
    [
        "whyce.operational.region.canary-started",
        "whyce.operational.region.fully-activated",
        "whyce.operational.region.halted",
        "whyce.operational.region.resumed",
        "whyce.global.health.reported",
        "whyce.coresystem.anomaly.detected"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILiveMonitoringViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();

        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);
        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var regionId = @event.Headers.TryGetValue("x-source-region", out var r) ? r : "unknown";

        var model = existing ?? new LiveMonitoringReadModel
        {
            Id = aggregateId,
            RegionId = regionId,
            ActivationStatus = "Unknown",
            TrafficPercent = 0,
            CommandsProcessed = 0,
            AnomaliesDetected = 0,
            GovernanceEscalations = 0,
            AvgLatencyMs = 0,
            HasCriticalAlerts = false,
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
