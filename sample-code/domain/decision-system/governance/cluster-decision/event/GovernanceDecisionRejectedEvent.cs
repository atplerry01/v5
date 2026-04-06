using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed record GovernanceDecisionRejectedEvent(
    Guid DecisionId,
    string Reason) : DomainEvent;
