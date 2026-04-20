using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed record MediaUploadProcessingStartedEvent(
    MediaUploadId UploadId,
    Timestamp StartedAt) : DomainEvent;
