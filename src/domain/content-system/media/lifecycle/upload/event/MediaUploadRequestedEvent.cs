using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed record MediaUploadRequestedEvent(
    MediaUploadId UploadId,
    MediaUploadSourceRef SourceRef,
    MediaUploadInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
