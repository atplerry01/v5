using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyAudit;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyAudit.Reducer;

public static class PolicyAuditProjectionReducer
{
    public static PolicyAuditReadModel Apply(PolicyAuditReadModel state, PolicyAuditEntryRecordedEventSchema e) =>
        state with
        {
            AuditId       = e.AggregateId,
            PolicyId      = e.PolicyId,
            ActorId       = e.ActorId,
            Action        = e.Action,
            Category      = e.Category,
            DecisionHash  = e.DecisionHash,
            CorrelationId = e.CorrelationId,
            OccurredAt    = e.OccurredAt,
            IsReviewed    = false
        };

    public static PolicyAuditReadModel Apply(PolicyAuditReadModel state, PolicyAuditEntryReviewedEventSchema e) =>
        state with
        {
            IsReviewed   = true,
            ReviewerId   = e.ReviewerId,
            ReviewReason = e.Reason
        };
}
