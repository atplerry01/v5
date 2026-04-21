using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Topology;

public sealed record DefineTopologyCommand(
    Guid TopologyId,
    Guid ClusterReference,
    string TopologyName) : IHasAggregateId
{
    public Guid AggregateId => TopologyId;
}

public sealed record ValidateTopologyCommand(
    Guid TopologyId) : IHasAggregateId
{
    public Guid AggregateId => TopologyId;
}

public sealed record LockTopologyCommand(
    Guid TopologyId) : IHasAggregateId
{
    public Guid AggregateId => TopologyId;
}
