using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed record CapitalAllocatedToSpvEvent(
    string AllocationId,
    string TargetId,
    decimal OwnershipPercentage) : DomainEvent;
