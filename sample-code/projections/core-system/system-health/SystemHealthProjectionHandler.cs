using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.CoreSystem.SystemHealth;

/// <summary>
/// Domain projection for system health. Event-driven ONLY.
/// Consumes system state events via Kafka.
/// NO domain imports, NO runtime imports, NO engine imports.
///
/// Performance:
/// - Version-skip: events older than last processed version are discarded.
/// - Idempotent: same event processed twice produces identical state.
/// - Supports full rebuild from event stream.
/// </summary>
public sealed class SystemHealthProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.coresystem.system-health";

    public string[] EventTypes =>
    [
        "whyce.coresystem.state.initialized",
        "whyce.coresystem.state.activated",
        "whyce.coresystem.state.snapshot-captured",
        "whyce.coresystem.state.validation-recorded",
        "whyce.coresystem.state.declared-authoritative",
        "whyce.coresystem.state.degraded"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISystemHealthViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();

        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);

        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var model = existing ?? new SystemHealthReadModel
        {
            Id = aggregateId,
            Status = "Initializing",
            EventStoreVersion = 0,
            ActiveAggregates = 0,
            TotalValidations = 0,
            FailedValidations = 0,
            IsAuthoritative = false,
            IsDegraded = false,
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
