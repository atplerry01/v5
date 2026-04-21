using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TemporalState;

public sealed class TemporalStateAggregate : AggregateRoot
{
    public static TemporalStateAggregate Create()
    {
        var aggregate = new TemporalStateAggregate();
        if (aggregate.Version >= 0)
            throw TemporalStateErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
