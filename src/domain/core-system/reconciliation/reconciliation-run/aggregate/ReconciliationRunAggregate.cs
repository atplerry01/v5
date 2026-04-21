using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationRun;

public sealed class ReconciliationRunAggregate : AggregateRoot
{
    public static ReconciliationRunAggregate Create()
    {
        var aggregate = new ReconciliationRunAggregate();
        if (aggregate.Version >= 0)
            throw ReconciliationRunErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
