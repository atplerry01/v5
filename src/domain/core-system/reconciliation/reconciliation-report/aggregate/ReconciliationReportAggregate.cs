namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationReport;

public sealed class ReconciliationReportAggregate
{
    public static ReconciliationReportAggregate Create()
    {
        var aggregate = new ReconciliationReportAggregate();
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
