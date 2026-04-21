using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed record MarkupActivatedEvent(
    [property: JsonPropertyName("AggregateId")] MarkupId MarkupId) : DomainEvent;
