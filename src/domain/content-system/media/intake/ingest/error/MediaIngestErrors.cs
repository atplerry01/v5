using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public static class MediaIngestErrors
{
    public static DomainException CannotAcceptUnlessRequested()
        => new("Media upload must be in requested state to be accepted.");

    public static DomainException CannotStartUnlessAccepted()
        => new("Media upload must be in accepted state to start processing.");

    public static DomainException CannotCompleteUnlessProcessing()
        => new("Media upload must be in processing state to complete.");

    public static DomainException AlreadyTerminal()
        => new("Media upload is already in a terminal state.");

    public static DomainException CannotCancelTerminal()
        => new("Cannot cancel a completed, failed, or cancelled upload.");
}
