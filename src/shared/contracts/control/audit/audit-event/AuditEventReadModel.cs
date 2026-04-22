namespace Whycespace.Shared.Contracts.Control.Audit.AuditEvent;

public sealed record AuditEventReadModel
{
    public Guid AuditEventId { get; init; }
    public string ActorId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public bool IsSealed { get; init; }
    public string? IntegrityHash { get; init; }
}
