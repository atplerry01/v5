namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(RateCardStatus status) => status == RateCardStatus.Active;
}
