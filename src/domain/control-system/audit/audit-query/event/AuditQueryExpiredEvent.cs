using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditQuery;

public sealed record AuditQueryExpiredEvent(
    AuditQueryId Id) : DomainEvent;
