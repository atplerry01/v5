using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed record MarkupCreatedEvent(
    [property: JsonPropertyName("AggregateId")] MarkupId MarkupId,
    MarkupCode Code,
    MarkupName Name,
    AdjustmentBasis Basis,
    MarkupAmount Amount) : DomainEvent;
