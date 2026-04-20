namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed class CanFinalizeSpecification
{
    public bool IsSatisfiedBy(QuoteBasisStatus status) => status == QuoteBasisStatus.Draft;
}
