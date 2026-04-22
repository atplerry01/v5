using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public static class EventMetadataErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("EventMetadata has already been initialized.");

    public static DomainException EnvelopeRefMissing() =>
        new DomainInvariantViolationException("EventMetadata requires a non-empty EnvelopeRef.");

    public static DomainException ActorIdMissing() =>
        new DomainInvariantViolationException("EventMetadata requires a non-empty ActorId.");

    public static DomainException ExecutionHashMissing() =>
        new DomainInvariantViolationException("EventMetadata requires a non-empty ExecutionHash.");

    public static DomainException PolicyDecisionHashMissing() =>
        new DomainInvariantViolationException("EventMetadata requires a non-empty PolicyDecisionHash.");
}
