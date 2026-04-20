namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(RateCardStatus status) => status == RateCardStatus.Draft;
}
