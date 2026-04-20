using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed record PriceBookCreatedEvent(
    PriceBookId PriceBookId,
    PriceBookName Name,
    PriceBookScopeRef? Scope,
    TimeWindow? Effective);
