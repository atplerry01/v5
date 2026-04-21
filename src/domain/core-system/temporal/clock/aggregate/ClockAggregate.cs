using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Clock;

public sealed class ClockAggregate : AggregateRoot
{
    public static ClockAggregate Create()
    {
        var aggregate = new ClockAggregate();
        if (aggregate.Version >= 0)
            throw ClockErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
