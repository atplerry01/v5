namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(DiscountRuleStatus status) => status == DiscountRuleStatus.Draft;
}
