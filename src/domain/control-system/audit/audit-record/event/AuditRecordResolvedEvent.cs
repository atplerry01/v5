using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditRecord;

public sealed record AuditRecordResolvedEvent(
    AuditRecordId Id,
    DateTimeOffset ResolvedAt) : DomainEvent;
