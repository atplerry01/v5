using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed record RateCardCreatedEvent(
    [property: JsonPropertyName("AggregateId")] RateCardId RateCardId,
    PriceBookRef PriceBook,
    RateCardName Name) : DomainEvent;
