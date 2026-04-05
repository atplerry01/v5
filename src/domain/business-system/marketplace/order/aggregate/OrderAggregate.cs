namespace Whycespace.Domain.BusinessSystem.Marketplace.Order;

public sealed class OrderAggregate
{
    public static OrderAggregate Create()
    {
        var aggregate = new OrderAggregate();
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
