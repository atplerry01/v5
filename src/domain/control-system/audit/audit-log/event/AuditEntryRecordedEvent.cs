using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditLog;

public sealed record AuditEntryRecordedEvent(
    AuditLogId Id,
    string ActorId,
    string Action,
    string Resource,
    AuditEntryClassification Classification,
    DateTimeOffset OccurredAt,
    string? DecisionId) : DomainEvent;
