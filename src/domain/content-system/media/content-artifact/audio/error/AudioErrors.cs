using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public static class AudioErrors
{
    public static DomainException AudioArchived()
        => new("Cannot mutate an archived audio.");

    public static DomainException AlreadyActive()
        => new("Audio is already active.");

    public static DomainException AlreadyArchived()
        => new("Audio is already archived.");

    public static DomainInvariantViolationException OrphanedAudio()
        => new("Audio must reference an owning media asset.");
}
