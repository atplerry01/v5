using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.AnomalyDashboard;

/// <summary>
/// Live anomaly dashboard projection. Tracks anomalies in real-time
/// across all regions. Event-driven ONLY. Idempotent, version-skip.
/// </summary>
public sealed class AnomalyDashboardProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.anomaly-dashboard";

    public string[] EventTypes =>
    [
        "whyce.coresystem.anomaly.detected",
        "whyce.coresystem.anomaly.resolved",
        "whyce.coresystem.anomaly.escalated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAnomalyDashboardViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();
        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);
        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var regionId = @event.Headers.TryGetValue("x-source-region", out var r) ? r : "unknown";

        var model = existing ?? new AnomalyDashboardReadModel
        {
            Id = aggregateId,
            RegionId = regionId,
            AnomalyType = @event.AggregateType,
            Severity = "Unknown",
            OccurrenceCount = 0,
            IsResolved = false,
            FirstDetected = @event.Timestamp,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model with
        {
            OccurrenceCount = model.OccurrenceCount + 1,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        }, ct);

        _versionTracker.MarkProcessed(aggregateId, @event.Version);
    }

    public void ResetForRebuild() => _versionTracker.Reset();
}
