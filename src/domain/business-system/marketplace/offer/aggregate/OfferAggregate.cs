namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed class OfferAggregate
{
    public static OfferAggregate Create()
    {
        var aggregate = new OfferAggregate();
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
