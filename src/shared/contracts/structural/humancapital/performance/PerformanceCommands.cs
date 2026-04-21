using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Performance;

public sealed record CreatePerformanceCommand(
    Guid PerformanceId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => PerformanceId;
}
