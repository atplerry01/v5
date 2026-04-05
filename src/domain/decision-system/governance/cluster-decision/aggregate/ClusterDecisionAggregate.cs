namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed class ClusterDecisionAggregate
{
    public static ClusterDecisionAggregate Create()
    {
        var aggregate = new ClusterDecisionAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
