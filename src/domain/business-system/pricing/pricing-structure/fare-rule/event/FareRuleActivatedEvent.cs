using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed record FareRuleActivatedEvent(
    [property: JsonPropertyName("AggregateId")] FareRuleId FareRuleId) : DomainEvent;
