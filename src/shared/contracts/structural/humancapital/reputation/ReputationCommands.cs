using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Reputation;

public sealed record CreateReputationCommand(
    Guid ReputationId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => ReputationId;
}
