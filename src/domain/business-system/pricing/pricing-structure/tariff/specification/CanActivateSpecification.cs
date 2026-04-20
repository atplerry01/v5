namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TariffStatus status) => status == TariffStatus.Draft;
}
