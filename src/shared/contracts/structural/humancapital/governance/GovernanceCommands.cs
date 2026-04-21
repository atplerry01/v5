using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Governance;

public sealed record CreateGovernanceCommand(
    Guid GovernanceId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => GovernanceId;
}
