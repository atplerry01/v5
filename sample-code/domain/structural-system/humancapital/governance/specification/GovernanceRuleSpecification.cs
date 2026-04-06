namespace Whycespace.Domain.StructuralSystem.HumanCapital.Governance;

public sealed class GovernanceRuleSpecification
{
    public bool IsSatisfiedBy(WorkforcePolicyAggregate policy) => policy.IsEnforced;
}
