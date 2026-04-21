using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;

public sealed record CreateAllocationCommand(
    Guid AllocationId,
    Guid ResourceId,
    int RequestedCapacity) : IHasAggregateId
{
    public Guid AggregateId => AllocationId;
}

public sealed record AllocateAllocationCommand(Guid AllocationId) : IHasAggregateId
{
    public Guid AggregateId => AllocationId;
}

public sealed record ReleaseAllocationCommand(Guid AllocationId) : IHasAggregateId
{
    public Guid AggregateId => AllocationId;
}
