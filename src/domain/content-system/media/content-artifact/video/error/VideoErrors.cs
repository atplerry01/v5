using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public static class VideoErrors
{
    public static DomainException VideoArchived()
        => new("Cannot mutate an archived video.");

    public static DomainException AlreadyActive()
        => new("Video is already active.");

    public static DomainException AlreadyArchived()
        => new("Video is already archived.");

    public static DomainInvariantViolationException OrphanedVideo()
        => new("Video must reference an owning media asset.");
}
