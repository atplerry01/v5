using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Escalation;

public sealed record EscalationInitializedEvent(
    SubjectId SubjectId,
    EscalationWindow Window,
    Timestamp InitializedAt) : DomainEvent;
