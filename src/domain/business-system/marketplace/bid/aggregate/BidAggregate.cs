namespace Whycespace.Domain.BusinessSystem.Marketplace.Bid;

public sealed class BidAggregate
{
    public static BidAggregate Create()
    {
        var aggregate = new BidAggregate();
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
