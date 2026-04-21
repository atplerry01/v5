using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.VarianceControl;

public sealed class VarianceControlAggregate : AggregateRoot
{
    public static VarianceControlAggregate Create()
    {
        var aggregate = new VarianceControlAggregate();
        if (aggregate.Version >= 0)
            throw VarianceControlErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
