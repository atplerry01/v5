using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public static class ProgressErrors
{
    public static DomainException ProgressAlreadyTerminated()
        => new("Playback progress is already terminated.");

    public static DomainException CannotPauseUnlessTracking()
        => new("Playback can only be paused when Tracking.");

    public static DomainException CannotResumeUnlessPaused()
        => new("Playback can only be resumed when Paused.");

    public static DomainInvariantViolationException MissingProgressId()
        => new("Progress requires a valid ProgressId.");

    public static DomainInvariantViolationException MissingSessionRef()
        => new("Progress requires a valid SessionRef.");
}
