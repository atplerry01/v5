namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PriceBookStatus status) => status == PriceBookStatus.Draft;
}
