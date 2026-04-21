using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed record MarkupUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] MarkupId MarkupId,
    MarkupName Name,
    AdjustmentBasis Basis,
    MarkupAmount Amount) : DomainEvent;
