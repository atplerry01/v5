using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed record RateEntryAddedEvent(
    [property: JsonPropertyName("AggregateId")] RateCardId RateCardId,
    RateEntry Entry) : DomainEvent;
