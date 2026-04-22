using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditEvent;

public sealed record AuditEventSealedEvent(
    AuditEventId Id,
    string IntegrityHash) : DomainEvent;
