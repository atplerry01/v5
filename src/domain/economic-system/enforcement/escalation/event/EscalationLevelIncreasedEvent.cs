using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Escalation;

public sealed record EscalationLevelIncreasedEvent(
    SubjectId SubjectId,
    EscalationLevel PreviousLevel,
    EscalationLevel NewLevel,
    ViolationCounter Counter,
    Timestamp EscalatedAt) : DomainEvent;
