namespace Whycespace.Domain.BusinessSystem.Inventory.Item;

public sealed class ItemAggregate
{
    public static ItemAggregate Create()
    {
        var aggregate = new ItemAggregate();
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
