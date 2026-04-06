using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Reconciliation.ReconciliationReport;

public sealed class ReconciliationReportProjectionHandler
{
    public string ProjectionName => "whyce.core.reconciliation.reconciliation-report";

    public string[] EventTypes =>
    [
        "whyce.core.reconciliation.reconciliation-report.created",
        "whyce.core.reconciliation.reconciliation-report.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReconciliationReportViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReconciliationReportReadModel
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
