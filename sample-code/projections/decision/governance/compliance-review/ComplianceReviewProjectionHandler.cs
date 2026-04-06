using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.ComplianceReview;

public sealed class ComplianceReviewProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.compliance-review";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.compliance-review.created",
        "whyce.decision.governance.compliance-review.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IComplianceReviewViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ComplianceReviewReadModel
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
