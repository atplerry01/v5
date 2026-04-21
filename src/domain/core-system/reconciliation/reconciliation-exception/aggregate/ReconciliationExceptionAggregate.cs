using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationException;

public sealed class ReconciliationExceptionAggregate : AggregateRoot
{
    public static ReconciliationExceptionAggregate Create()
    {
        var aggregate = new ReconciliationExceptionAggregate();
        if (aggregate.Version >= 0)
            throw ReconciliationExceptionErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
