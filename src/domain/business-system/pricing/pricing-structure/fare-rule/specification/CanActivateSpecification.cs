namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(FareRuleStatus status) => status == FareRuleStatus.Draft;
}
