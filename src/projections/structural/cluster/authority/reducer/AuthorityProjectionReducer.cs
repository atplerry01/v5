using Whycespace.Shared.Contracts.Events.Structural.Cluster.Authority;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;

namespace Whycespace.Projections.Structural.Cluster.Authority.Reducer;

public static class AuthorityProjectionReducer
{
    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthorityEstablishedEventSchema e, DateTimeOffset at) =>
        state with
        {
            AuthorityId = e.AggregateId,
            ClusterReference = e.ClusterReference,
            AuthorityName = e.AuthorityName,
            Status = "Established",
            LastModifiedAt = at
        };

    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthorityAttachedEventSchema e, DateTimeOffset at) =>
        state with
        {
            AuthorityId = e.AggregateId,
            AttachedClusterRef = e.ClusterRef,
            AttachedAt = e.EffectiveAt,
            LastModifiedAt = at
        };

    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthorityBindingValidatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            AuthorityId = e.AggregateId,
            BindingValidated = true,
            BindingParent = e.Parent,
            BindingParentState = e.ParentState,
            LastModifiedAt = at
        };

    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthorityActivatedEventSchema e, DateTimeOffset at) =>
        state with { AuthorityId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthorityRevokedEventSchema e, DateTimeOffset at) =>
        state with { AuthorityId = e.AggregateId, Status = "Revoked", LastModifiedAt = at };

    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthoritySuspendedEventSchema e, DateTimeOffset at) =>
        state with { AuthorityId = e.AggregateId, Status = "Suspended", LastModifiedAt = at };

    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthorityReactivatedEventSchema e, DateTimeOffset at) =>
        state with { AuthorityId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static AuthorityReadModel Apply(AuthorityReadModel state, AuthorityRetiredEventSchema e, DateTimeOffset at) =>
        state with { AuthorityId = e.AggregateId, Status = "Retired", LastModifiedAt = at };
}
