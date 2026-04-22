using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;

public sealed record PolicyAuditEntryRecordedEvent(
    PolicyAuditId Id,
    string PolicyId,
    string ActorId,
    string Action,
    PolicyAuditCategory Category,
    string DecisionHash,
    string CorrelationId,
    DateTimeOffset OccurredAt) : DomainEvent;
