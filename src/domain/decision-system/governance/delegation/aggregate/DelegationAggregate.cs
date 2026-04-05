namespace Whycespace.Domain.DecisionSystem.Governance.Delegation;

public sealed class DelegationAggregate
{
    public static DelegationAggregate Create()
    {
        var aggregate = new DelegationAggregate();
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
