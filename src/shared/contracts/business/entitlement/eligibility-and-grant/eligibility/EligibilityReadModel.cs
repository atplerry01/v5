namespace Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record EligibilityReadModel
{
    public Guid EligibilityId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid TargetId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
