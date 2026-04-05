namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public sealed class RebalanceAggregate
{
    public static RebalanceAggregate Create()
    {
        var aggregate = new RebalanceAggregate();
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
