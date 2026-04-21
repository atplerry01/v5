using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventCatalog;

public static class EventCatalogErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("EventCatalog has already been initialized.");
}
