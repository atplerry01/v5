namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteAcceptedEvent(QuoteId QuoteId, DateTimeOffset AcceptedAt);
