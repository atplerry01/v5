namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;

public sealed record PolicyAuditReadModel
{
    public Guid AuditId { get; init; }
    public string PolicyId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string DecisionHash { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public bool IsReviewed { get; init; }
    public string? ReviewerId { get; init; }
    public string? ReviewReason { get; init; }
}
