namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(SurchargeStatus status) => status == SurchargeStatus.Active;
}
