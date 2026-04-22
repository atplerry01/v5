using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Rule;

public sealed record EnforcementRuleDisabledEvent(
    RuleId RuleId) : DomainEvent;
