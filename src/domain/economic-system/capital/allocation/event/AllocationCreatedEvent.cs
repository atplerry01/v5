using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed record AllocationCreatedEvent(
    AllocationId AllocationId,
    Guid SourceAccountId,
    TargetId TargetId,
    Amount Amount,
    Currency Currency,
    Timestamp AllocatedAt) : DomainEvent;
