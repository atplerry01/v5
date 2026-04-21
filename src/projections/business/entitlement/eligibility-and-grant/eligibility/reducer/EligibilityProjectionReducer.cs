using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Eligibility;

namespace Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Eligibility.Reducer;

public static class EligibilityProjectionReducer
{
    public static EligibilityReadModel Apply(EligibilityReadModel state, EligibilityCreatedEventSchema e) =>
        state with
        {
            EligibilityId = e.AggregateId,
            SubjectId = e.SubjectId,
            TargetId = e.TargetId,
            Scope = e.Scope,
            Status = "Pending"
        };

    public static EligibilityReadModel Apply(EligibilityReadModel state, EligibilityEvaluatedEligibleEventSchema e) =>
        state with
        {
            EligibilityId = e.AggregateId,
            Status = "Eligible",
            Reason = null,
            LastUpdatedAt = e.EvaluatedAt
        };

    public static EligibilityReadModel Apply(EligibilityReadModel state, EligibilityEvaluatedIneligibleEventSchema e) =>
        state with
        {
            EligibilityId = e.AggregateId,
            Status = "Ineligible",
            Reason = e.Reason,
            LastUpdatedAt = e.EvaluatedAt
        };
}
