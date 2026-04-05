namespace Whycespace.Domain.DecisionSystem.Governance.Quorum;

public sealed class QuorumAggregate
{
    public static QuorumAggregate Create()
    {
        var aggregate = new QuorumAggregate();
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
