namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationException;

public sealed class ReconciliationExceptionAggregate
{
    public static ReconciliationExceptionAggregate Create()
    {
        var aggregate = new ReconciliationExceptionAggregate();
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
