namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(SurchargeStatus status) => status == SurchargeStatus.Draft;
}
