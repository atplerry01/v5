namespace Whycespace.Domain.CoreSystem.Financialcontrol.ReserveControl;

public sealed class ReserveControlAggregate
{
    public static ReserveControlAggregate Create()
    {
        var aggregate = new ReserveControlAggregate();
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
