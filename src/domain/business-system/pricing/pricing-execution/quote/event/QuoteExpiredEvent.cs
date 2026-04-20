namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteExpiredEvent(QuoteId QuoteId, DateTimeOffset ExpiredAt);
