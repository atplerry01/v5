namespace Whycespace.Domain.StructuralSystem.HumanCapital.Eligibility;

public sealed class EligibilityRuleSpecification
{
    public bool IsSatisfiedBy(EligibilityAggregate eligibility) => eligibility.IsEligible;
}
