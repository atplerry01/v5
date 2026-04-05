namespace Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

public sealed class ReconciliationAggregate
{
    public static ReconciliationAggregate Create()
    {
        var aggregate = new ReconciliationAggregate();
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
