using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Approval;

public sealed class ApprovalProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.approval";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.approval.created",
        "whyce.business.agreement.approval.updated"
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
