using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventStream;

public static class EventStreamErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("EventStream has already been initialized.");

    public static DomainException SourceRouteMissing() =>
        new DomainInvariantViolationException("EventStream requires a valid SourceRoute.");

    public static DomainException EventTypeSetEmpty() =>
        new DomainInvariantViolationException("EventStream requires at least one included event type.");
}
