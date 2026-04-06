using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Financialcontrol.ApprovalControl;

public sealed class ApprovalControlProjectionHandler
{
    public string ProjectionName => "whyce.core.financialcontrol.approval-control";

    public string[] EventTypes =>
    [
        "whyce.core.financialcontrol.approval-control.created",
        "whyce.core.financialcontrol.approval-control.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IApprovalControlViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ApprovalControlReadModel
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
