namespace Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;

public sealed record RestrictionReadModel
{
    public Guid RestrictionId { get; init; }
    public Guid SubjectId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? RemovedAt { get; init; }
    public string RemovalReason { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
