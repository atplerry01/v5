using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed record RateCardCreatedEvent(
    RateCardId RateCardId,
    PriceBookRef PriceBook,
    RateCardName Name);
