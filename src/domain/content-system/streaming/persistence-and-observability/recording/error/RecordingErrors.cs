using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public static class RecordingErrors
{
    public static DomainException CannotCompleteAfterFailure()
        => new("Cannot complete a recording that has already failed.");

    public static DomainException CannotCompleteUnlessStarted()
        => new("Recording must be in started state to complete.");

    public static DomainException CannotFailTerminal()
        => new("Recording is already in a terminal state.");

    public static DomainException CannotFinalizeUnlessCompleted()
        => new("Recording must be completed before it can be finalized.");

    public static DomainException CannotArchiveUnlessFinalized()
        => new("Recording must be finalized before it can be archived.");

    public static DomainException AlreadyFinalized()
        => new("Recording is already finalized.");

    public static DomainException AlreadyArchived()
        => new("Recording is already archived.");

    public static DomainInvariantViolationException OrphanedRecording()
        => new("Recording must reference a parent stream.");
}
