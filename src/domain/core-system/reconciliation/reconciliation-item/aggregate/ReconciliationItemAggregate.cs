using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationItem;

public sealed class ReconciliationItemAggregate : AggregateRoot
{
    public static ReconciliationItemAggregate Create()
    {
        var aggregate = new ReconciliationItemAggregate();
        if (aggregate.Version >= 0)
            throw ReconciliationItemErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
