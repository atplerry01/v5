namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteRejectedEvent(QuoteId QuoteId, DateTimeOffset RejectedAt);
