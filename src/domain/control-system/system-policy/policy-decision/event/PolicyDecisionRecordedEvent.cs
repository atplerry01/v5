using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDecision;

public sealed record PolicyDecisionRecordedEvent(
    PolicyDecisionId Id,
    string PolicyDefinitionId,
    string SubjectId,
    string Action,
    string Resource,
    PolicyDecisionOutcome Outcome,
    DateTimeOffset DecidedAt) : DomainEvent;
