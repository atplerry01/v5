using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed record GovernanceDecisionApprovedEvent(
    Guid DecisionId,
    Guid ClusterId) : DomainEvent;
