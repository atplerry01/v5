using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Timeline;

public sealed class TimelineAggregate : AggregateRoot
{
    public static TimelineAggregate Create()
    {
        var aggregate = new TimelineAggregate();
        if (aggregate.Version >= 0)
            throw TimelineErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
