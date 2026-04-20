using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Processing;

public sealed record DocumentProcessingCompletedEvent(
    ProcessingJobId JobId,
    ProcessingOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
