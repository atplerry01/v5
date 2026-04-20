using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadFailedEvent(
    DocumentUploadId UploadId,
    DocumentUploadFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
