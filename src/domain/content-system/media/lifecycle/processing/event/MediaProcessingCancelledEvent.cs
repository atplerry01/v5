using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public sealed record MediaProcessingCancelledEvent(
    MediaProcessingJobId JobId,
    Timestamp CancelledAt) : DomainEvent;
