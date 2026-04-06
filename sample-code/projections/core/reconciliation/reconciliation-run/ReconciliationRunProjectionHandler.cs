using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Reconciliation.ReconciliationRun;

public sealed class ReconciliationRunProjectionHandler
{
    public string ProjectionName => "whyce.core.reconciliation.reconciliation-run";

    public string[] EventTypes =>
    [
        "whyce.core.reconciliation.reconciliation-run.created",
        "whyce.core.reconciliation.reconciliation-run.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReconciliationRunViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReconciliationRunReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
