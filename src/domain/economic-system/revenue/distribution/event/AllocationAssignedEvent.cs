using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record AllocationAssignedEvent(
    DistributionId DistributionId,
    Guid RecipientId,
    Amount AllocationAmount,
    decimal SharePercentage) : DomainEvent;
