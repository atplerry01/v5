namespace Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEvaluation;

public sealed record PolicyEvaluationRecordedEventSchema(
    Guid AggregateId,
    string PolicyId,
    string ActorId,
    string Action,
    string CorrelationId);

public sealed record PolicyEvaluationVerdictIssuedEventSchema(
    Guid AggregateId,
    string Outcome,
    string DecisionHash);
