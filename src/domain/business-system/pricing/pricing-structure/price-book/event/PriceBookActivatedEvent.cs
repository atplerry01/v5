using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed record PriceBookActivatedEvent(
    [property: JsonPropertyName("AggregateId")] PriceBookId PriceBookId,
    TimeWindow Effective) : DomainEvent;
