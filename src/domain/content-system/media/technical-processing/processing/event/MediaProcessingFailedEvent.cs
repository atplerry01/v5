using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingFailedEvent(
    MediaProcessingJobId JobId,
    MediaProcessingFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
