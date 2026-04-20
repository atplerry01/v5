using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadCancelledEvent(
    DocumentUploadId UploadId,
    Timestamp CancelledAt) : DomainEvent;
