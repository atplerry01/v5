namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed class CanAcceptSpecification
{
    public bool IsSatisfiedBy(QuoteStatus status) => status == QuoteStatus.Issued;
}
