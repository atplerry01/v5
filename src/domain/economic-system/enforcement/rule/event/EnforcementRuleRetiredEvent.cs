using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

public sealed record EnforcementRuleRetiredEvent(
    RuleId RuleId) : DomainEvent;
