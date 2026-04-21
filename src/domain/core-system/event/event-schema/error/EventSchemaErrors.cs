using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventSchema;

public static class EventSchemaErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("EventSchema has already been initialized.");
}
