using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed record GovernanceDecisionExecutedEvent(
    Guid DecisionId,
    Guid ClusterId) : DomainEvent;
