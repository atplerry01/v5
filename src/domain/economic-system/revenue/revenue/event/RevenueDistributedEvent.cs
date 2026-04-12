using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed record RevenueDistributedEvent(
    RevenueId RevenueId,
    Timestamp DistributedAt) : DomainEvent;
