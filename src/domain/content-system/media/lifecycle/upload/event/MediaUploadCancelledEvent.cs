using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed record MediaUploadCancelledEvent(
    MediaUploadId UploadId,
    Timestamp CancelledAt) : DomainEvent;
