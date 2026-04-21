using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventCatalog;

public sealed class EventCatalogAggregate : AggregateRoot
{
    public static EventCatalogAggregate Create()
    {
        var aggregate = new EventCatalogAggregate();
        if (aggregate.Version >= 0)
            throw EventCatalogErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }
}
