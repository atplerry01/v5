using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public static class EventSchemaErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("EventSchema has already been initialized.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("EventSchema has already been deprecated.");
}
