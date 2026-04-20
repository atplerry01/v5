using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingCancelledEvent(
    MediaProcessingJobId JobId,
    Timestamp CancelledAt) : DomainEvent;
