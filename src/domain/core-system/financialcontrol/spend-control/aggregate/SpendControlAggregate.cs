using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.SpendControl;

public sealed class SpendControlAggregate : AggregateRoot
{
    public static SpendControlAggregate Create()
    {
        var aggregate = new SpendControlAggregate();
        if (aggregate.Version >= 0)
            throw SpendControlErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
