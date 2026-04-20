using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public sealed record MediaProcessingRequestedEvent(
    MediaProcessingJobId JobId,
    MediaProcessingKind Kind,
    MediaProcessingInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
