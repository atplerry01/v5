namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

public sealed class GuardianAggregate
{
    public static GuardianAggregate Create()
    {
        var aggregate = new GuardianAggregate();
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
