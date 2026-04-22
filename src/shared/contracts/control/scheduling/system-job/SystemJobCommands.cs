using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;

public sealed record DefineSystemJobCommand(
    Guid JobId,
    string Name,
    string Category,
    TimeSpan Timeout) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record DeprecateSystemJobCommand(
    Guid JobId) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}
