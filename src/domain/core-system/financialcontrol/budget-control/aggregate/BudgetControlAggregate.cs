using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.BudgetControl;

public sealed class BudgetControlAggregate : AggregateRoot
{
    public static BudgetControlAggregate Create()
    {
        var aggregate = new BudgetControlAggregate();
        if (aggregate.Version >= 0)
            throw BudgetControlErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
