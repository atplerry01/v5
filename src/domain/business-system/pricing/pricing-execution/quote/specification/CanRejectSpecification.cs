namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(QuoteStatus status) => status == QuoteStatus.Issued;
}
