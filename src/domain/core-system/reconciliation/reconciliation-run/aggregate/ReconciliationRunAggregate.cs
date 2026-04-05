namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationRun;

public sealed class ReconciliationRunAggregate
{
    public static ReconciliationRunAggregate Create()
    {
        var aggregate = new ReconciliationRunAggregate();
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
