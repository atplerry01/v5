using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditTrace;

public sealed record AuditTraceOpenedEvent(
    AuditTraceId Id,
    string CorrelationId,
    DateTimeOffset OpenedAt) : DomainEvent;
