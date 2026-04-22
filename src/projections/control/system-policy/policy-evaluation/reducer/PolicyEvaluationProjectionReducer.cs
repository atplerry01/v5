using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEvaluation;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyEvaluation.Reducer;

public static class PolicyEvaluationProjectionReducer
{
    public static PolicyEvaluationReadModel Apply(PolicyEvaluationReadModel state, PolicyEvaluationRecordedEventSchema e) =>
        state with
        {
            EvaluationId  = e.AggregateId,
            PolicyId      = e.PolicyId,
            ActorId       = e.ActorId,
            Action        = e.Action,
            CorrelationId = e.CorrelationId
        };

    public static PolicyEvaluationReadModel Apply(PolicyEvaluationReadModel state, PolicyEvaluationVerdictIssuedEventSchema e) =>
        state with
        {
            Outcome      = e.Outcome,
            DecisionHash = e.DecisionHash
        };
}
