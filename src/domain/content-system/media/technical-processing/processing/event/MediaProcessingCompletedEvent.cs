using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingCompletedEvent(
    MediaProcessingJobId JobId,
    MediaProcessingOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
