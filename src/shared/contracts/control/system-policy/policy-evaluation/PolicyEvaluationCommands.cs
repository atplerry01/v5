using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;

public sealed record RecordPolicyEvaluationCommand(
    Guid EvaluationId,
    string PolicyId,
    string ActorId,
    string Action,
    string CorrelationId) : IHasAggregateId
{
    public Guid AggregateId => EvaluationId;
}

public sealed record IssuePolicyEvaluationVerdictCommand(
    Guid EvaluationId,
    string Outcome,
    string DecisionHash) : IHasAggregateId
{
    public Guid AggregateId => EvaluationId;
}
