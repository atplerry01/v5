namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(QuoteBasisStatus status) => status != QuoteBasisStatus.Archived;
}
