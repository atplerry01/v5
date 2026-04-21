using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed record DiscountRuleDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] DiscountRuleId DiscountRuleId) : DomainEvent;
