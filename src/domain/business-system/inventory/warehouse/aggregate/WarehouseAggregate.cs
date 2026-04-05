namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public sealed class WarehouseAggregate
{
    public static WarehouseAggregate Create()
    {
        var aggregate = new WarehouseAggregate();
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
