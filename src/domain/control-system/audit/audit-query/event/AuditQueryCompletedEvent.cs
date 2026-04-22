using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditQuery;

public sealed record AuditQueryCompletedEvent(
    AuditQueryId Id,
    int ResultCount) : DomainEvent;
