using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEnforcement;

public sealed record PolicyEnforcedEvent(
    PolicyEnforcementId Id,
    string PolicyDecisionId,
    string TargetId,
    PolicyEnforcementOutcome Outcome,
    DateTimeOffset EnforcedAt,
    bool IsNoPolicyFlagAnomaly) : DomainEvent;
