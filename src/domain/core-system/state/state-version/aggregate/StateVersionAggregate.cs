using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateVersion;

public sealed class StateVersionAggregate : AggregateRoot
{
    public static StateVersionAggregate Create()
    {
        var aggregate = new StateVersionAggregate();
        if (aggregate.Version >= 0)
            throw StateVersionErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
