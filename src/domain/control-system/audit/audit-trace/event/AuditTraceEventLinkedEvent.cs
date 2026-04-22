using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditTrace;

public sealed record AuditTraceEventLinkedEvent(
    AuditTraceId Id,
    string AuditEventId) : DomainEvent;
