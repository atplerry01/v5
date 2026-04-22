using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public static class CommandMetadataErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("CommandMetadata has already been initialized.");

    public static DomainException EnvelopeRefMissing() =>
        new DomainInvariantViolationException("CommandMetadata requires a non-empty EnvelopeRef.");

    public static DomainException ActorIdMissing() =>
        new DomainInvariantViolationException("CommandMetadata requires a non-empty ActorId. Anonymous commands are forbidden.");
}
