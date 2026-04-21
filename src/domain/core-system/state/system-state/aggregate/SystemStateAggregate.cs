using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.SystemState;

public sealed class SystemStateAggregate : AggregateRoot
{
    public static SystemStateAggregate Create()
    {
        var aggregate = new SystemStateAggregate();
        if (aggregate.Version >= 0)
            throw SystemStateErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
