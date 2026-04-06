using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.CoreSystem.ReconciliationStatus;

/// <summary>
/// Domain projection for reconciliation status. Event-driven ONLY.
/// Consumes reconciliation verification events via Kafka.
/// NO domain imports, NO runtime imports, NO engine imports.
///
/// Performance:
/// - Version-skip: events older than last processed version are discarded.
/// - Idempotent: same event processed twice produces identical state.
/// - Supports full rebuild from event stream.
/// </summary>
public sealed class ReconciliationStatusProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.coresystem.reconciliation-status";

    public string[] EventTypes =>
    [
        "whyce.coresystem.reconciliation.session-started",
        "whyce.coresystem.reconciliation.check-recorded",
        "whyce.coresystem.reconciliation.session-completed",
        "whyce.coresystem.reconciliation.session-failed"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReconciliationStatusViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();

        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);

        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var model = existing ?? new ReconciliationStatusReadModel
        {
            Id = aggregateId,
            ScopeType = string.Empty,
            TargetSystem = string.Empty,
            Status = "Pending",
            TotalChecks = 0,
            FailedChecks = 0,
            AllConsistent = true,
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
