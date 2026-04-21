using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.GlobalInvariant;

public sealed class GlobalInvariantAggregate : AggregateRoot
{
    public static GlobalInvariantAggregate Create()
    {
        var aggregate = new GlobalInvariantAggregate();
        if (aggregate.Version >= 0)
            throw GlobalInvariantErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
