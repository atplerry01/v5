using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed record RateCardArchivedEvent(
    [property: JsonPropertyName("AggregateId")] RateCardId RateCardId) : DomainEvent;
