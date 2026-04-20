using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Processing;

public static class DocumentProcessingErrors
{
    public static DomainException JobAlreadyStarted()
        => new("Processing job has already started.");

    public static DomainException JobNotRequested()
        => new("Processing job must be in requested state to start.");

    public static DomainException JobNotRunning()
        => new("Processing job is not running.");

    public static DomainException JobAlreadyTerminal()
        => new("Processing job is already in a terminal state.");

    public static DomainException CannotCancelTerminal()
        => new("Cannot cancel a job that has already completed, failed, or been cancelled.");
}
