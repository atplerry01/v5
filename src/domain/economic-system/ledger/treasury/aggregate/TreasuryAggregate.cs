namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed class TreasuryAggregate
{
    public static TreasuryAggregate Create()
    {
        var aggregate = new TreasuryAggregate();
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
