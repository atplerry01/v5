using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed record MarkupCreatedEvent(
    MarkupId MarkupId,
    MarkupCode Code,
    MarkupName Name,
    AdjustmentBasis Basis,
    MarkupAmount Amount);
