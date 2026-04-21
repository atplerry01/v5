using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateProjection;

public sealed class StateProjectionAggregate : AggregateRoot
{
    public static StateProjectionAggregate Create()
    {
        var aggregate = new StateProjectionAggregate();
        if (aggregate.Version >= 0)
            throw StateProjectionErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
