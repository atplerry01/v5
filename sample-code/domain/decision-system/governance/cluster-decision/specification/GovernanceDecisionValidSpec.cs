namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed class GovernanceDecisionValidSpec
{
    public bool IsSatisfiedBy(ClusterGovernanceDecisionAggregate decision)
    {
        return decision.ClusterId != Guid.Empty
            && !string.IsNullOrWhiteSpace(decision.DecisionType)
            && !string.IsNullOrWhiteSpace(decision.DecisionHash);
    }
}
