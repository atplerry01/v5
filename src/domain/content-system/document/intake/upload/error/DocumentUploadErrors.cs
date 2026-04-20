using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public static class DocumentUploadErrors
{
    public static DomainException CannotAcceptUnlessRequested()
        => new("Document upload must be in requested state to be accepted.");

    public static DomainException CannotStartUnlessAccepted()
        => new("Document upload must be in accepted state to start processing.");

    public static DomainException CannotCompleteUnlessProcessing()
        => new("Document upload must be in processing state to complete.");

    public static DomainException AlreadyTerminal()
        => new("Document upload is already in a terminal state.");

    public static DomainException CannotCancelTerminal()
        => new("Cannot cancel a completed, failed, or cancelled upload.");
}
