using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadAcceptedEvent(
    DocumentUploadId UploadId,
    Timestamp AcceptedAt) : DomainEvent;
