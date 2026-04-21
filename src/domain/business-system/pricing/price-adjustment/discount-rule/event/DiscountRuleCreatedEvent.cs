using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed record DiscountRuleCreatedEvent(
    [property: JsonPropertyName("AggregateId")] DiscountRuleId DiscountRuleId,
    DiscountRuleCode Code,
    DiscountRuleName Name,
    AdjustmentBasis Basis,
    DiscountAmount Amount) : DomainEvent;
