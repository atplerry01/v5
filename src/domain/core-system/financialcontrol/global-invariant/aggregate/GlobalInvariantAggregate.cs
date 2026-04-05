namespace Whycespace.Domain.CoreSystem.Financialcontrol.GlobalInvariant;

public sealed class GlobalInvariantAggregate
{
    public static GlobalInvariantAggregate Create()
    {
        var aggregate = new GlobalInvariantAggregate();
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
