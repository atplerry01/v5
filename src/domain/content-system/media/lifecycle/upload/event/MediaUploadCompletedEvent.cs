using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed record MediaUploadCompletedEvent(
    MediaUploadId UploadId,
    MediaUploadOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
