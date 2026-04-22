using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditEvent;

public sealed record AuditEventCapturedEvent(
    AuditEventId Id,
    string ActorId,
    string Action,
    AuditEventKind Kind,
    string CorrelationId,
    DateTimeOffset OccurredAt) : DomainEvent;
