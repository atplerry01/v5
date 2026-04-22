using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDecision;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyDecision.Reducer;

public static class PolicyDecisionProjectionReducer
{
    public static PolicyDecisionReadModel Apply(PolicyDecisionReadModel state, PolicyDecisionRecordedEventSchema e) =>
        state with
        {
            DecisionId       = e.AggregateId,
            PolicyDefinitionId = e.PolicyDefinitionId,
            SubjectId        = e.SubjectId,
            Action           = e.Action,
            Resource         = e.Resource,
            Outcome          = e.Outcome,
            DecidedAt        = e.DecidedAt
        };
}
