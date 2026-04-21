namespace Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentReadModel
{
    public Guid AssignmentId { get; init; }
    public Guid GrantId { get; init; }
    public Guid SubjectId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
