using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed record RateCardActivatedEvent(RateCardId RateCardId, TimeWindow Effective);
