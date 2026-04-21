using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Sanction;

public sealed record CreateSanctionCommand(
    Guid SanctionId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => SanctionId;
}
