using Whycespace.Shared.Contracts.Events.Structural.Cluster.Administration;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;

namespace Whycespace.Projections.Structural.Cluster.Administration.Reducer;

public static class AdministrationProjectionReducer
{
    public static AdministrationReadModel Apply(AdministrationReadModel state, AdministrationEstablishedEventSchema e, DateTimeOffset at) =>
        state with
        {
            AdministrationId = e.AggregateId,
            ClusterReference = e.ClusterReference,
            AdministrationName = e.AdministrationName,
            Status = "Established",
            LastModifiedAt = at
        };

    public static AdministrationReadModel Apply(AdministrationReadModel state, AdministrationAttachedEventSchema e, DateTimeOffset at) =>
        state with
        {
            AdministrationId = e.AggregateId,
            AttachedClusterRef = e.ClusterRef,
            AttachedAt = e.EffectiveAt,
            LastModifiedAt = at
        };

    public static AdministrationReadModel Apply(AdministrationReadModel state, AdministrationBindingValidatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            AdministrationId = e.AggregateId,
            BindingValidated = true,
            BindingParent = e.Parent,
            BindingParentState = e.ParentState,
            LastModifiedAt = at
        };

    public static AdministrationReadModel Apply(AdministrationReadModel state, AdministrationActivatedEventSchema e, DateTimeOffset at) =>
        state with { AdministrationId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static AdministrationReadModel Apply(AdministrationReadModel state, AdministrationSuspendedEventSchema e, DateTimeOffset at) =>
        state with { AdministrationId = e.AggregateId, Status = "Suspended", LastModifiedAt = at };

    public static AdministrationReadModel Apply(AdministrationReadModel state, AdministrationRetiredEventSchema e, DateTimeOffset at) =>
        state with { AdministrationId = e.AggregateId, Status = "Retired", LastModifiedAt = at };
}
