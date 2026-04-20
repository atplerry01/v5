using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed record MediaUploadFailedEvent(
    MediaUploadId UploadId,
    MediaUploadFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
