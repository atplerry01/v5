namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(PriceBookStatus status) => status == PriceBookStatus.Active;
}
