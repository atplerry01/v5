using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadCompletedEvent(
    DocumentUploadId UploadId,
    DocumentUploadOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
