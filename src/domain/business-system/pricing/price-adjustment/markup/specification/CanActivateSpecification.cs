namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(MarkupStatus status) => status == MarkupStatus.Draft;
}
