namespace Whycespace.Projections.Decision.Governance.ComplianceReview;

public sealed record ComplianceReviewView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
