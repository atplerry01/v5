using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

public static class PlaybackErrors
{
    public static DomainException PlaybackAlreadyEnabled()
        => new("Playback is already enabled.");

    public static DomainException PlaybackAlreadyDisabled()
        => new("Playback is already disabled.");

    public static DomainException PlaybackArchived()
        => new("Cannot mutate an archived playback.");

    public static DomainException PlaybackAlreadyArchived()
        => new("Playback is already archived.");

    public static DomainException InvalidDisableReason()
        => new("Disable reason cannot be empty.");

    public static DomainInvariantViolationException OrphanedPlayback()
        => new("Playback must reference a valid source (stream, session, or recording).");
}
