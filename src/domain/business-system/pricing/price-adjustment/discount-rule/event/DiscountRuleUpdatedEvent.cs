using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed record DiscountRuleUpdatedEvent(
    DiscountRuleId DiscountRuleId,
    DiscountRuleName Name,
    AdjustmentBasis Basis,
    DiscountAmount Amount);
