namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteCreatedEvent(
    QuoteId QuoteId,
    QuoteBasisRef QuoteBasis,
    QuoteReference Reference);
