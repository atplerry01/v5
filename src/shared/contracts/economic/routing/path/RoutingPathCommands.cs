using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Routing.Path;

public sealed record DefineRoutingPathCommand(
    Guid PathId,
    string PathType,
    string Conditions,
    int Priority) : IHasAggregateId
{
    public Guid AggregateId => PathId;
}

public sealed record ActivateRoutingPathCommand(Guid PathId) : IHasAggregateId
{
    public Guid AggregateId => PathId;
}

public sealed record DisableRoutingPathCommand(Guid PathId) : IHasAggregateId
{
    public Guid AggregateId => PathId;
}
