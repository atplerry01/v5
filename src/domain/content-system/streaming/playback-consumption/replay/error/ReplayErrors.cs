using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public static class ReplayErrors
{
    public static DomainException ReplayAlreadyTerminal()
        => new("Replay is already in a terminal state (Completed or Abandoned).");

    public static DomainException CannotStartUnlessRequested()
        => new("Replay can only be started from Requested status.");

    public static DomainException CannotPauseUnlessActive()
        => new("Replay can only be paused when Active.");

    public static DomainException CannotResumeUnlessPaused()
        => new("Replay can only be resumed when Paused.");

    public static DomainException CannotCompleteUnlessActive()
        => new("Replay can only be completed when Active.");

    public static DomainInvariantViolationException MissingReplayId()
        => new("Replay requires a valid ReplayId.");

    public static DomainInvariantViolationException MissingArchiveRef()
        => new("Replay requires a valid ArchiveRef.");
}
