namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed class FulfillmentAggregate
{
    public static FulfillmentAggregate Create()
    {
        var aggregate = new FulfillmentAggregate();
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
