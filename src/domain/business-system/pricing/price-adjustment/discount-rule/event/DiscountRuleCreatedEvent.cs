using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed record DiscountRuleCreatedEvent(
    DiscountRuleId DiscountRuleId,
    DiscountRuleCode Code,
    DiscountRuleName Name,
    AdjustmentBasis Basis,
    DiscountAmount Amount);
