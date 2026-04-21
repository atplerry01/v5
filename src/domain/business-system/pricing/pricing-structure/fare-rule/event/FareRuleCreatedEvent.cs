using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed record FareRuleCreatedEvent(
    [property: JsonPropertyName("AggregateId")] FareRuleId FareRuleId,
    TariffRef Tariff,
    FareRuleCode Code,
    FareRuleCondition Condition) : DomainEvent;
