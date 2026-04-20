namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(PriceBookStatus status) => status != PriceBookStatus.Archived;
}
