using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public static class TranscriptErrors
{
    public static DomainException TranscriptFinalized()
        => new("Transcript is finalized and cannot be modified.");

    public static DomainException TranscriptArchived()
        => new("Cannot mutate an archived transcript.");

    public static DomainException AlreadyArchived()
        => new("Transcript is already archived.");

    public static DomainException AlreadyFinalized()
        => new("Transcript is already finalized.");

    public static DomainInvariantViolationException OrphanedTranscript()
        => new("Transcript must reference an owning media asset.");
}
