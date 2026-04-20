using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingStartedEvent(
    MediaProcessingJobId JobId,
    Timestamp StartedAt) : DomainEvent;
