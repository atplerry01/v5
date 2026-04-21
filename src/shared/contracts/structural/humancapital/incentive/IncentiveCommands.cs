using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Incentive;

public sealed record CreateIncentiveCommand(
    Guid IncentiveId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => IncentiveId;
}
