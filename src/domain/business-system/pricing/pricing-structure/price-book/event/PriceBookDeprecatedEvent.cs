using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed record PriceBookDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] PriceBookId PriceBookId) : DomainEvent;
