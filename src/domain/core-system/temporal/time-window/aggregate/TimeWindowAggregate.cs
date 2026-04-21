using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

public sealed class TimeWindowAggregate : AggregateRoot
{
    public static TimeWindowAggregate Create()
    {
        var aggregate = new TimeWindowAggregate();
        if (aggregate.Version >= 0)
            throw TimeWindowErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
