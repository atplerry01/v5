using Whycespace.Shared.Contracts.Events.Structural.Cluster.Lifecycle;
using Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;

namespace Whycespace.Projections.Structural.Cluster.Lifecycle.Reducer;

public static class LifecycleProjectionReducer
{
    public static LifecycleReadModel Apply(LifecycleReadModel state, LifecycleDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            LifecycleId = e.AggregateId,
            ClusterReference = e.ClusterReference,
            LifecycleName = e.LifecycleName,
            Status = "Defined",
            LastModifiedAt = at
        };

    public static LifecycleReadModel Apply(LifecycleReadModel state, LifecycleTransitionedEventSchema e, DateTimeOffset at) =>
        state with { LifecycleId = e.AggregateId, Status = "Transitioned", LastModifiedAt = at };

    public static LifecycleReadModel Apply(LifecycleReadModel state, LifecycleCompletedEventSchema e, DateTimeOffset at) =>
        state with { LifecycleId = e.AggregateId, Status = "Completed", LastModifiedAt = at };
}
