namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed class CanReviseSpecification
{
    public bool IsSatisfiedBy(QuoteBasisStatus status) => status == QuoteBasisStatus.Draft;
}
