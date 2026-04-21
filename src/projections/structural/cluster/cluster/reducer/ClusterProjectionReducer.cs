using Whycespace.Shared.Contracts.Events.Structural.Cluster.Cluster;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Projections.Structural.Cluster.Cluster.Reducer;

public static class ClusterProjectionReducer
{
    public static ClusterReadModel Apply(ClusterReadModel state, ClusterDefinedEventSchema e, DateTimeOffset at) =>
        state with
        {
            ClusterId = e.AggregateId,
            ClusterName = e.ClusterName,
            ClusterType = e.ClusterType,
            Status = "Defined",
            LastModifiedAt = at
        };

    public static ClusterReadModel Apply(ClusterReadModel state, ClusterActivatedEventSchema e, DateTimeOffset at) =>
        state with { ClusterId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static ClusterReadModel Apply(ClusterReadModel state, ClusterArchivedEventSchema e, DateTimeOffset at) =>
        state with { ClusterId = e.AggregateId, Status = "Archived", LastModifiedAt = at };

    public static ClusterReadModel Apply(ClusterReadModel state, ClusterAuthorityBoundEventSchema e, DateTimeOffset at)
    {
        var next = new HashSet<Guid>(state.ActiveAuthorityIds) { e.AuthorityId };
        return state with
        {
            ClusterId = e.AggregateId,
            ActiveAuthorityIds = next.ToArray(),
            LastModifiedAt = at
        };
    }

    public static ClusterReadModel Apply(ClusterReadModel state, ClusterAuthorityReleasedEventSchema e, DateTimeOffset at)
    {
        var next = new HashSet<Guid>(state.ActiveAuthorityIds);
        next.Remove(e.AuthorityId);
        return state with
        {
            ClusterId = e.AggregateId,
            ActiveAuthorityIds = next.ToArray(),
            LastModifiedAt = at
        };
    }

    public static ClusterReadModel Apply(ClusterReadModel state, ClusterAdministrationBoundEventSchema e, DateTimeOffset at)
    {
        var next = new HashSet<Guid>(state.ActiveAdministrationIds) { e.AdministrationId };
        return state with
        {
            ClusterId = e.AggregateId,
            ActiveAdministrationIds = next.ToArray(),
            LastModifiedAt = at
        };
    }

    public static ClusterReadModel Apply(ClusterReadModel state, ClusterAdministrationReleasedEventSchema e, DateTimeOffset at)
    {
        var next = new HashSet<Guid>(state.ActiveAdministrationIds);
        next.Remove(e.AdministrationId);
        return state with
        {
            ClusterId = e.AggregateId,
            ActiveAdministrationIds = next.ToArray(),
            LastModifiedAt = at
        };
    }
}
