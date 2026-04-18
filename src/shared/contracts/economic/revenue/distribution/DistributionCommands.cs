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

public sealed record ConfirmDistributionCommand(
    Guid DistributionId) : IHasAggregateId
{
    public Guid AggregateId => DistributionId;
}

public sealed record MarkDistributionPaidCommand(
    Guid DistributionId,
    Guid PayoutId) : IHasAggregateId
{
    public Guid AggregateId => DistributionId;
}

public sealed record MarkDistributionFailedCommand(
    Guid DistributionId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => DistributionId;
}

public sealed record RequestDistributionCompensationCommand(
    Guid DistributionId,
    Guid OriginalPayoutId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => DistributionId;
}

public sealed record MarkDistributionCompensatedCommand(
    Guid DistributionId,
    Guid OriginalPayoutId,
    string CompensatingJournalId) : IHasAggregateId
{
    public Guid AggregateId => DistributionId;
}
