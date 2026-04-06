using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Reconciliation.ReconciliationException;

public sealed class ReconciliationExceptionProjectionHandler
{
    public string ProjectionName => "whyce.core.reconciliation.reconciliation-exception";

    public string[] EventTypes =>
    [
        "whyce.core.reconciliation.reconciliation-exception.created",
        "whyce.core.reconciliation.reconciliation-exception.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReconciliationExceptionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReconciliationExceptionReadModel
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
