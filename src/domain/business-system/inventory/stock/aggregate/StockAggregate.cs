namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public sealed class StockAggregate
{
    public static StockAggregate Create()
    {
        var aggregate = new StockAggregate();
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
