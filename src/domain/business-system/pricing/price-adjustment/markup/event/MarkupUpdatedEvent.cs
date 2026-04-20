using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed record MarkupUpdatedEvent(
    MarkupId MarkupId,
    MarkupName Name,
    AdjustmentBasis Basis,
    MarkupAmount Amount);
