namespace Whycespace.Domain.IntelligenceSystem.Economic.Kernel;

public sealed class KernelAggregate
{
    public static KernelAggregate Create()
    {
        var aggregate = new KernelAggregate();
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
