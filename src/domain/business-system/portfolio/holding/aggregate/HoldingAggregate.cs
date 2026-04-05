namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public sealed class HoldingAggregate
{
    public static HoldingAggregate Create()
    {
        var aggregate = new HoldingAggregate();
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
