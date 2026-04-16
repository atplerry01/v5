using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Capital.Allocation;

public sealed record CreateCapitalAllocationCommand(
    Guid AllocationId,
    Guid SourceAccountId,
    Guid TargetId,
    decimal Amount,
    string Currency,
    DateTimeOffset AllocatedAt) : IHasAggregateId
{
    public Guid AggregateId => AllocationId;
}

public sealed record ReleaseCapitalAllocationCommand(
    Guid AllocationId,
    DateTimeOffset ReleasedAt) : IHasAggregateId
{
    public Guid AggregateId => AllocationId;
}

public sealed record CompleteCapitalAllocationCommand(
    Guid AllocationId,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => AllocationId;
}

public sealed record AllocateCapitalToSpvCommand(
    Guid AllocationId,
    string SpvTargetId,
    decimal OwnershipPercentage) : IHasAggregateId
{
    public Guid AggregateId => AllocationId;
}
