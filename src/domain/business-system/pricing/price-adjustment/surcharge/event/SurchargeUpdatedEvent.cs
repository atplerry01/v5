using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed record SurchargeUpdatedEvent(
    SurchargeId SurchargeId,
    SurchargeName Name,
    AdjustmentBasis Basis,
    SurchargeAmount Amount);
