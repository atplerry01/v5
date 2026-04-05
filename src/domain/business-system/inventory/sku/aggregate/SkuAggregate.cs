namespace Whycespace.Domain.BusinessSystem.Inventory.Sku;

public sealed class SkuAggregate
{
    public static SkuAggregate Create()
    {
        var aggregate = new SkuAggregate();
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
