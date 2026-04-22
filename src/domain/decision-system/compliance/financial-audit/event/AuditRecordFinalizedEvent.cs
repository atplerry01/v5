using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Compliance.Audit;

public sealed record AuditRecordFinalizedEvent(
    AuditRecordId AuditRecordId,
    Timestamp FinalizedAt) : DomainEvent;
