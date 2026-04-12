using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

public sealed class IsActiveSpecification : Specification<EnforcementRuleAggregate>
{
    public override bool IsSatisfiedBy(EnforcementRuleAggregate rule) =>
        rule.Status == RuleStatus.Active;
}
