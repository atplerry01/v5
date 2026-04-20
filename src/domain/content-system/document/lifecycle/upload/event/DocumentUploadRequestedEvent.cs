using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public sealed record DocumentUploadRequestedEvent(
    DocumentUploadId UploadId,
    DocumentUploadSourceRef SourceRef,
    DocumentUploadInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
