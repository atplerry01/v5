using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.CoreSystem.FinancialIntegrity;

/// <summary>
/// Domain projection for financial integrity. Event-driven ONLY.
/// Consumes financial control events via Kafka and builds the read model.
/// NO domain imports, NO runtime imports, NO engine imports.
///
/// Performance:
/// - Version-skip: events older than last processed version are discarded.
/// - Idempotent: same event processed twice produces identical state.
/// - Supports full rebuild from event stream.
/// </summary>
public sealed class FinancialIntegrityProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.coresystem.financial-integrity";

    public string[] EventTypes =>
    [
        "whyce.coresystem.financialcontrol.initialized",
        "whyce.coresystem.financialcontrol.inflow-recorded",
        "whyce.coresystem.financialcontrol.outflow-recorded",
        "whyce.coresystem.financialcontrol.vault-verified",
        "whyce.coresystem.financialcontrol.balance-violation",
        "whyce.coresystem.financialcontrol.sealed"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IFinancialIntegrityViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();

        // Version-skip: discard already-processed events
        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);

        // Idempotency: skip if existing model is at or beyond this version
        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var model = @event.EventType switch
        {
            "whyce.coresystem.financialcontrol.initialized" => new FinancialIntegrityReadModel
            {
                Id = aggregateId,
                TotalInflow = 0m,
                TotalOutflow = 0m,
                SystemBalance = 0m,
                IsBalanced = true,
                IsNegativeBalance = false,
                LastUpdated = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            },
            _ => existing ?? new FinancialIntegrityReadModel
            {
                Id = aggregateId,
                TotalInflow = 0m,
                TotalOutflow = 0m,
                SystemBalance = 0m,
                IsBalanced = true,
                IsNegativeBalance = false,
                LastUpdated = @event.Timestamp,
                LastEventTimestamp = @event.Timestamp,
                LastEventVersion = @event.Version
            }
        };

        await repository.SaveAsync(model with
        {
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        }, ct);

        _versionTracker.MarkProcessed(aggregateId, @event.Version);
    }

    /// <summary>
    /// Resets version tracking for a full projection rebuild.
    /// </summary>
    public void ResetForRebuild() => _versionTracker.Reset();
}
