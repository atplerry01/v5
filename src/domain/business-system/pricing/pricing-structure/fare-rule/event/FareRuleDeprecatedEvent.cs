using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed record FareRuleDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] FareRuleId FareRuleId) : DomainEvent;
