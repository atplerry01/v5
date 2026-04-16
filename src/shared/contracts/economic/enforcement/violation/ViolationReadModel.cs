namespace Whycespace.Shared.Contracts.Economic.Enforcement.Violation;

public sealed record ViolationReadModel
{
    public Guid ViolationId { get; init; }
    public Guid RuleId { get; init; }
    public Guid SourceReference { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string EnforcementStatus { get; init; } = string.Empty;
    public string Resolution { get; init; } = string.Empty;
    public DateTimeOffset DetectedAt { get; init; }
    public DateTimeOffset? AcknowledgedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
