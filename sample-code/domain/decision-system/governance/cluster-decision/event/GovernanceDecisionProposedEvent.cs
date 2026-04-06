using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed record GovernanceDecisionProposedEvent(
    Guid DecisionId,
    Guid ClusterId,
    string DecisionType) : DomainEvent;
