using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public sealed record PriceDefinedEvent(
    PricingId PricingId,
    Guid ContractId,
    PricingModel Model,
    Amount Price,
    Currency Currency,
    Timestamp DefinedAt) : DomainEvent;
