using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed record QuoteBasisCreatedEvent(
    QuoteBasisId QuoteBasisId,
    PriceBookRef PriceBook,
    QuoteBasisContext Context);
