using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEnforcement;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyEnforcement.Reducer;

public static class PolicyEnforcementProjectionReducer
{
    public static PolicyEnforcementReadModel Apply(PolicyEnforcementReadModel state, PolicyEnforcedEventSchema e) =>
        state with
        {
            EnforcementId        = e.AggregateId,
            PolicyDecisionId     = e.PolicyDecisionId,
            TargetId             = e.TargetId,
            Outcome              = e.Outcome,
            EnforcedAt           = e.EnforcedAt,
            IsNoPolicyFlagAnomaly = e.IsNoPolicyFlagAnomaly
        };
}
