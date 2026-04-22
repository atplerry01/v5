namespace Whycespace.Shared.Contracts.Control.Audit.AuditLog;

public sealed record AuditLogReadModel
{
    public Guid AuditLogId { get; init; }
    public string ActorId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Resource { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public string? DecisionId { get; init; }
}
