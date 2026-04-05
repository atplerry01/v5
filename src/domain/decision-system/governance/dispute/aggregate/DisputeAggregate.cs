namespace Whycespace.Domain.DecisionSystem.Governance.Dispute;

public sealed class DisputeAggregate
{
    public static DisputeAggregate Create()
    {
        var aggregate = new DisputeAggregate();
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
