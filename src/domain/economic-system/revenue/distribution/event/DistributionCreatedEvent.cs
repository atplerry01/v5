using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionCreatedEvent(
    DistributionId DistributionId,
    Guid RevenueId,
    Amount TotalAmount,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
