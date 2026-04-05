namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class PayoutAggregate
{
    public static PayoutAggregate Create()
    {
        var aggregate = new PayoutAggregate();
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
