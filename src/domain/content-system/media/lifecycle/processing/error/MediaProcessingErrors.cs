using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public static class MediaProcessingErrors
{
    public static DomainException JobAlreadyStarted()
        => new("Media processing job has already started.");

    public static DomainException JobNotRequested()
        => new("Media processing job must be in requested state to start.");

    public static DomainException JobNotRunning()
        => new("Media processing job is not running.");

    public static DomainException JobAlreadyTerminal()
        => new("Media processing job is already in a terminal state.");

    public static DomainException CannotCancelTerminal()
        => new("Cannot cancel a job that has already completed, failed, or been cancelled.");
}
