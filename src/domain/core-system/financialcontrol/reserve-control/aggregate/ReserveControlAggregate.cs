using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.ReserveControl;

public sealed class ReserveControlAggregate : AggregateRoot
{
    public static ReserveControlAggregate Create()
    {
        var aggregate = new ReserveControlAggregate();
        if (aggregate.Version >= 0)
            throw ReserveControlErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
