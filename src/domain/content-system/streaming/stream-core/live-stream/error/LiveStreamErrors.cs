using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public static class LiveStreamErrors
{
    public static DomainException CannotScheduleAfterStart()
        => new("Live stream cannot be (re)scheduled after it has started.");

    public static DomainException CannotStartTerminal()
        => new("Live stream is already ended or cancelled.");

    public static DomainException CannotStartLive()
        => new("Live stream is already live.");

    public static DomainException CannotPauseUnlessLive()
        => new("Only a live stream can be paused.");

    public static DomainException CannotResumeUnlessPaused()
        => new("Only a paused live stream can be resumed.");

    public static DomainException CannotEndUnlessActive()
        => new("Only a live or paused live stream can be ended.");

    public static DomainException CannotCancelTerminal()
        => new("Live stream is already ended or cancelled.");

    public static DomainException InvalidCancellationReason()
        => new("Cancellation reason cannot be empty.");

    public static DomainInvariantViolationException OrphanedLiveStream()
        => new("Live stream must reference a parent stream.");
}
