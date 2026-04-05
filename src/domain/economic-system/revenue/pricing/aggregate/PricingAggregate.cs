namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public sealed class PricingAggregate
{
    public static PricingAggregate Create()
    {
        var aggregate = new PricingAggregate();
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
