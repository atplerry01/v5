using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Revenue.Distribution;

public sealed record DistributionAllocation(string ParticipantId, decimal OwnershipPercentage);

public sealed record CreateDistributionCommand(
    Guid DistributionId,
    string SpvId,
    decimal TotalAmount,
    IReadOnlyList<DistributionAllocation> Allocations) : IHasAggregateId
{
    public Guid AggregateId => DistributionId;
}
