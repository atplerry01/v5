using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public sealed record DocumentUploadProcessingStartedEvent(
    DocumentUploadId UploadId,
    Timestamp StartedAt) : DomainEvent;
