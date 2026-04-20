namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(RateCardStatus status, int entryCount)
        => status == RateCardStatus.Draft && entryCount > 0;
}
