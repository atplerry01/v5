namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(TariffStatus status) => status == TariffStatus.Active;
}
