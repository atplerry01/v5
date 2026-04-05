namespace Whycespace.Domain.BusinessSystem.Inventory.Batch;

public sealed class BatchAggregate
{
    public static BatchAggregate Create()
    {
        var aggregate = new BatchAggregate();
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
