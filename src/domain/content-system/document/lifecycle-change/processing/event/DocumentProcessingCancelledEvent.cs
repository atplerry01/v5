using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingCancelledEvent(
    ProcessingJobId JobId,
    Timestamp CancelledAt) : DomainEvent;
