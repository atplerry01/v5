using Whycespace.Shared.Contracts.Events.Structural.Cluster.Subcluster;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Projections.Structural.Cluster.Subcluster.Reducer;

public static class SubclusterProjectionReducer
{
    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SubclusterId = e.AggregateId,
            ParentClusterReference = e.ParentClusterReference,
            SubclusterName = e.SubclusterName,
            Status = "Defined",
            LastModifiedAt = at
        };

    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterAttachedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SubclusterId = e.AggregateId,
            AttachedClusterRef = e.ClusterRef,
            AttachedAt = e.EffectiveAt,
            LastModifiedAt = at
        };

    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterBindingValidatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SubclusterId = e.AggregateId,
            BindingValidated = true,
            BindingParent = e.Parent,
            BindingParentState = e.ParentState,
            LastModifiedAt = at
        };

    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterActivatedEventSchema e, DateTimeOffset at) =>
        state with { SubclusterId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterSuspendedEventSchema e, DateTimeOffset at) =>
        state with { SubclusterId = e.AggregateId, Status = "Suspended", LastModifiedAt = at };

    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterReactivatedEventSchema e, DateTimeOffset at) =>
        state with { SubclusterId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterArchivedEventSchema e, DateTimeOffset at) =>
        state with { SubclusterId = e.AggregateId, Status = "Archived", LastModifiedAt = at };

    public static SubclusterReadModel Apply(SubclusterReadModel state, SubclusterRetiredEventSchema e, DateTimeOffset at) =>
        state with { SubclusterId = e.AggregateId, Status = "Retired", LastModifiedAt = at };
}
