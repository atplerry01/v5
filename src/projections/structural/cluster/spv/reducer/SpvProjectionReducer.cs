using Whycespace.Shared.Contracts.Events.Structural.Cluster.Spv;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;

namespace Whycespace.Projections.Structural.Cluster.Spv.Reducer;

public static class SpvProjectionReducer
{
    public static SpvReadModel Apply(SpvReadModel state, SpvCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SpvId = e.AggregateId,
            ClusterReference = e.ClusterReference,
            SpvName = e.SpvName,
            SpvType = e.SpvType,
            Status = "Created",
            LastModifiedAt = at
        };

    public static SpvReadModel Apply(SpvReadModel state, SpvAttachedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SpvId = e.AggregateId,
            AttachedClusterRef = e.ClusterRef,
            AttachedAt = e.EffectiveAt,
            LastModifiedAt = at
        };

    public static SpvReadModel Apply(SpvReadModel state, SpvBindingValidatedEventSchema e, DateTimeOffset at) =>
        state with { SpvId = e.AggregateId, LastModifiedAt = at };

    public static SpvReadModel Apply(SpvReadModel state, SpvActivatedEventSchema e, DateTimeOffset at) =>
        state with { SpvId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static SpvReadModel Apply(SpvReadModel state, SpvSuspendedEventSchema e, DateTimeOffset at) =>
        state with { SpvId = e.AggregateId, Status = "Suspended", LastModifiedAt = at };

    public static SpvReadModel Apply(SpvReadModel state, SpvClosedEventSchema e, DateTimeOffset at) =>
        state with { SpvId = e.AggregateId, Status = "Closed", LastModifiedAt = at };

    public static SpvReadModel Apply(SpvReadModel state, SpvReactivatedEventSchema e, DateTimeOffset at) =>
        state with { SpvId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static SpvReadModel Apply(SpvReadModel state, SpvRetiredEventSchema e, DateTimeOffset at) =>
        state with { SpvId = e.AggregateId, Status = "Retired", LastModifiedAt = at };
}
