using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed record MediaUploadAcceptedEvent(
    MediaUploadId UploadId,
    Timestamp AcceptedAt) : DomainEvent;
