namespace Whycespace.Domain.IntelligenceSystem.Economic.Autonomy;

public sealed record AutonomousDecisionEvaluatedEvent(
    Guid DecisionId,
    string DecisionHash) : DomainEvent;
