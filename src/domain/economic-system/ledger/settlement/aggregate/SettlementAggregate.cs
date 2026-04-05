namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementAggregate
{
    public static SettlementAggregate Create()
    {
        var aggregate = new SettlementAggregate();
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
