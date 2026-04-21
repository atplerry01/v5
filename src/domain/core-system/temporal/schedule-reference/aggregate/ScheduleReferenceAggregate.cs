using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.ScheduleReference;

public sealed class ScheduleReferenceAggregate : AggregateRoot
{
    public static ScheduleReferenceAggregate Create()
    {
        var aggregate = new ScheduleReferenceAggregate();
        if (aggregate.Version >= 0)
            throw ScheduleReferenceErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
