using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed record PriceBookCreatedEvent(
    [property: JsonPropertyName("AggregateId")] PriceBookId PriceBookId,
    PriceBookName Name,
    PriceBookScopeRef? Scope,
    TimeWindow? Effective) : DomainEvent;
