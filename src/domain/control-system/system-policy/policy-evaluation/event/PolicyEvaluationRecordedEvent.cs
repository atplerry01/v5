using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;

public sealed record PolicyEvaluationRecordedEvent(
    PolicyEvaluationId Id,
    string PolicyId,
    string ActorId,
    string Action,
    string CorrelationId) : DomainEvent;
