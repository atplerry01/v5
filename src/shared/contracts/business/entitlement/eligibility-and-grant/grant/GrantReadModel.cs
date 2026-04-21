namespace Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantReadModel
{
    public Guid GrantId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid TargetId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
