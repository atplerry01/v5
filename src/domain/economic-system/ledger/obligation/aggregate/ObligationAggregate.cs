namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed class ObligationAggregate
{
    public static ObligationAggregate Create()
    {
        var aggregate = new ObligationAggregate();
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
