using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationReport;

public sealed class ReconciliationReportAggregate : AggregateRoot
{
    public static ReconciliationReportAggregate Create()
    {
        var aggregate = new ReconciliationReportAggregate();
        if (aggregate.Version >= 0)
            throw ReconciliationReportErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
