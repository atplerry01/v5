using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Grant;

namespace Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Grant.Reducer;

public static class GrantProjectionReducer
{
    public static GrantReadModel Apply(GrantReadModel state, GrantCreatedEventSchema e) =>
        state with
        {
            GrantId = e.AggregateId,
            SubjectId = e.SubjectId,
            TargetId = e.TargetId,
            Scope = e.Scope,
            Status = "Pending",
            ExpiresAt = e.ExpiresAt
        };

    public static GrantReadModel Apply(GrantReadModel state, GrantActivatedEventSchema e) =>
        state with
        {
            GrantId = e.AggregateId,
            Status = "Active",
            LastUpdatedAt = e.ActivatedAt
        };

    public static GrantReadModel Apply(GrantReadModel state, GrantRevokedEventSchema e) =>
        state with
        {
            GrantId = e.AggregateId,
            Status = "Revoked",
            LastUpdatedAt = e.RevokedAt
        };

    public static GrantReadModel Apply(GrantReadModel state, GrantExpiredEventSchema e) =>
        state with
        {
            GrantId = e.AggregateId,
            Status = "Expired",
            LastUpdatedAt = e.ExpiredAt
        };
}
