using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public static class EventEnvelopeErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("EventEnvelopeId is required and must not be empty.");

    public static DomainException MissingMetadata()
        => new DomainInvariantViolationException("Event envelope must include valid metadata.");

    public static DomainException InvalidStateTransition(EventEnvelopeStatus current, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("EventEnvelope has already been initialized.");
}
