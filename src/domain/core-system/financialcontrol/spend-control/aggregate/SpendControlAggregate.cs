namespace Whycespace.Domain.CoreSystem.Financialcontrol.SpendControl;

public sealed class SpendControlAggregate
{
    public static SpendControlAggregate Create()
    {
        var aggregate = new SpendControlAggregate();
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
