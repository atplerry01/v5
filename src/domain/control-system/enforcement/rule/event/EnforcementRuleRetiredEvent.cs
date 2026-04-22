using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Rule;

public sealed record EnforcementRuleRetiredEvent(
    RuleId RuleId) : DomainEvent;
