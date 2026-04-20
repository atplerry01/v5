namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(MarkupStatus status) => status == MarkupStatus.Active;
}
