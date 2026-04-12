using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed record AllocationReleasedEvent(
    AllocationId AllocationId,
    Guid SourceAccountId,
    Amount ReleasedAmount,
    Timestamp ReleasedAt) : DomainEvent;
