using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventSchema;

public sealed class EventSchemaAggregate : AggregateRoot
{
    public static EventSchemaAggregate Create()
    {
        var aggregate = new EventSchemaAggregate();
        if (aggregate.Version >= 0)
            throw EventSchemaErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
