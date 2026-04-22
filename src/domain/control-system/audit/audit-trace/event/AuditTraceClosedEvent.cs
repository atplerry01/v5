using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditTrace;

public sealed record AuditTraceClosedEvent(
    AuditTraceId Id,
    DateTimeOffset ClosedAt) : DomainEvent;
