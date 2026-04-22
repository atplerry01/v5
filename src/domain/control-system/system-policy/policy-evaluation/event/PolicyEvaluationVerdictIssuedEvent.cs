using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;

public sealed record PolicyEvaluationVerdictIssuedEvent(
    PolicyEvaluationId Id,
    EvaluationOutcome Outcome,
    string DecisionHash) : DomainEvent;
