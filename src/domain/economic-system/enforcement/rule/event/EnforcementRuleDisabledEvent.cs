using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

public sealed record EnforcementRuleDisabledEvent(
    RuleId RuleId) : DomainEvent;
