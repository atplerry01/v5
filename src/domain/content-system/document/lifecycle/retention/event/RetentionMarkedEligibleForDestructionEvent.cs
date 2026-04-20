using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public sealed record RetentionMarkedEligibleForDestructionEvent(
    RetentionId RetentionId,
    Timestamp MarkedAt) : DomainEvent;
