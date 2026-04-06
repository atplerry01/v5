using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Reconciliation.ReconciliationItem;

public sealed class ReconciliationItemProjectionHandler
{
    public string ProjectionName => "whyce.core.reconciliation.reconciliation-item";

    public string[] EventTypes =>
    [
        "whyce.core.reconciliation.reconciliation-item.created",
        "whyce.core.reconciliation.reconciliation-item.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReconciliationItemViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReconciliationItemReadModel
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
