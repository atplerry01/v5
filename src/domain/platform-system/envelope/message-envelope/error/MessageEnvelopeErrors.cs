using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public static class MessageEnvelopeErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("MessageEnvelope has already been initialized.");

    public static DomainException AlreadyDispatched() =>
        new DomainInvariantViolationException("MessageEnvelope has already been dispatched.");

    public static DomainException AlreadyTerminated() =>
        new DomainInvariantViolationException("MessageEnvelope is in a terminal state (Acknowledged or Rejected) and cannot be modified.");

    public static DomainException NotDispatched() =>
        new DomainInvariantViolationException("MessageEnvelope must be in Dispatched state to acknowledge or reject.");
}
