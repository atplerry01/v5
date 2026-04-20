using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingStartedEvent(
    ProcessingJobId JobId,
    Timestamp StartedAt) : DomainEvent;
