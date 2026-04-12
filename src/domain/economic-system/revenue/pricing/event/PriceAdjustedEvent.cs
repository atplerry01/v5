using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public sealed record PriceAdjustedEvent(
    PricingId PricingId,
    Amount PreviousPrice,
    Amount NewPrice,
    string Reason,
    Timestamp AdjustedAt) : DomainEvent;
