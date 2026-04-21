using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Assignment;

namespace Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Assignment.Reducer;

public static class AssignmentProjectionReducer
{
    public static AssignmentReadModel Apply(AssignmentReadModel state, AssignmentCreatedEventSchema e) =>
        state with
        {
            AssignmentId = e.AggregateId,
            GrantId = e.GrantId,
            SubjectId = e.SubjectId,
            Scope = e.Scope,
            Status = "Pending"
        };

    public static AssignmentReadModel Apply(AssignmentReadModel state, AssignmentActivatedEventSchema e) =>
        state with
        {
            AssignmentId = e.AggregateId,
            Status = "Active",
            LastUpdatedAt = e.ActivatedAt
        };

    public static AssignmentReadModel Apply(AssignmentReadModel state, AssignmentRevokedEventSchema e) =>
        state with
        {
            AssignmentId = e.AggregateId,
            Status = "Revoked",
            LastUpdatedAt = e.RevokedAt
        };
}
