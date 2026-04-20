using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed record SurchargeCreatedEvent(
    SurchargeId SurchargeId,
    SurchargeCode Code,
    SurchargeName Name,
    AdjustmentBasis Basis,
    SurchargeAmount Amount);
