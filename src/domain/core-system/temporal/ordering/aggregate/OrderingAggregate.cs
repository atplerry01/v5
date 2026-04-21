using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

public sealed class OrderingAggregate : AggregateRoot
{
    public static OrderingAggregate Create()
    {
        var aggregate = new OrderingAggregate();
        if (aggregate.Version >= 0)
            throw OrderingErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
