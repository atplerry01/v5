using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Stewardship;

public sealed record CreateStewardshipCommand(
    Guid StewardshipId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => StewardshipId;
}
