using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingRequestedEvent(
    ProcessingJobId JobId,
    ProcessingKind Kind,
    ProcessingInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
