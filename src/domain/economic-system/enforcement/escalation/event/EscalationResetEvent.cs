using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Escalation;

public sealed record EscalationResetEvent(
    SubjectId SubjectId,
    EscalationWindow NewWindow,
    Timestamp ResetAt) : DomainEvent;
