using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public sealed record MediaProcessingStartedEvent(
    MediaProcessingJobId JobId,
    Timestamp StartedAt) : DomainEvent;
