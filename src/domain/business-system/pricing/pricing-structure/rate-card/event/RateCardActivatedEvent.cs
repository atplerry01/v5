using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed record RateCardActivatedEvent(
    [property: JsonPropertyName("AggregateId")] RateCardId RateCardId,
    TimeWindow Effective) : DomainEvent;
