namespace Whycespace.Domain.BusinessSystem.Execution.Allocation;

public sealed class AllocationAggregate
{
    public static AllocationAggregate Create()
    {
        var aggregate = new AllocationAggregate();
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
