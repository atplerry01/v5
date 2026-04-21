using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public static class EventStreamErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("EventStreamId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("StreamDescriptor is required and must not be default.");

    public static DomainException InvalidStateTransition(EventStreamStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("EventStream has already been initialized.");
}
