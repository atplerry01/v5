namespace Whycespace.Platform.Api.Decision.Governance.ComplianceReview;

public sealed record ComplianceReviewRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ComplianceReviewResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
