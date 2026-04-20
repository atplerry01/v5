using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public sealed record MediaProcessingCompletedEvent(
    MediaProcessingJobId JobId,
    MediaProcessingOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
