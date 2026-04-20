namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(QuoteStatus status) => status == QuoteStatus.Issued;
}
