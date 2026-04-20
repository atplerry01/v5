using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingFailedEvent(
    ProcessingJobId JobId,
    ProcessingFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
