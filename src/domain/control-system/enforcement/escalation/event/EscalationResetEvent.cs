using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Escalation;

public sealed record EscalationResetEvent(
    SubjectId SubjectId,
    EscalationWindow NewWindow,
    Timestamp ResetAt) : DomainEvent;
