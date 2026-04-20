namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(FareRuleStatus status) => status == FareRuleStatus.Active;
}
