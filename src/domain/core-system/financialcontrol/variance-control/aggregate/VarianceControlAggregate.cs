namespace Whycespace.Domain.CoreSystem.Financialcontrol.VarianceControl;

public sealed class VarianceControlAggregate
{
    public static VarianceControlAggregate Create()
    {
        var aggregate = new VarianceControlAggregate();
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
