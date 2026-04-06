using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Approval;

public sealed class ApprovalProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.approval";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.approval.created",
        "whyce.decision.governance.approval.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IApprovalViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ApprovalReadModel
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
