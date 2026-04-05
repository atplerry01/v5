namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public sealed class WriteoffAggregate
{
    public static WriteoffAggregate Create()
    {
        var aggregate = new WriteoffAggregate();
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
