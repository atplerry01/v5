using Whycespace.Shared.Contracts.Events.Structural.Cluster.Topology;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;

namespace Whycespace.Projections.Structural.Cluster.Topology.Reducer;

public static class TopologyProjectionReducer
{
    public static TopologyReadModel Apply(TopologyReadModel state, TopologyDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            TopologyId = e.AggregateId,
            ClusterReference = e.ClusterReference,
            TopologyName = e.TopologyName,
            Status = "Defined",
            LastModifiedAt = at
        };

    public static TopologyReadModel Apply(TopologyReadModel state, TopologyValidatedEventSchema e, DateTimeOffset at) =>
        state with { TopologyId = e.AggregateId, Status = "Validated", LastModifiedAt = at };

    public static TopologyReadModel Apply(TopologyReadModel state, TopologyLockedEventSchema e, DateTimeOffset at) =>
        state with { TopologyId = e.AggregateId, Status = "Locked", LastModifiedAt = at };
}
