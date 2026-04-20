namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed class CanIssueSpecification
{
    public bool IsSatisfiedBy(QuoteStatus status) => status == QuoteStatus.Draft;
}
