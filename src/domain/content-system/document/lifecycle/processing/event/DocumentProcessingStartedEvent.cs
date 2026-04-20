using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Processing;

public sealed record DocumentProcessingStartedEvent(
    ProcessingJobId JobId,
    Timestamp StartedAt) : DomainEvent;
