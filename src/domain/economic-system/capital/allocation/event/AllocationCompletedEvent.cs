using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed record AllocationCompletedEvent(
    AllocationId AllocationId,
    Timestamp CompletedAt) : DomainEvent;
