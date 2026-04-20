namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(DiscountRuleStatus status) => status == DiscountRuleStatus.Active;
}
