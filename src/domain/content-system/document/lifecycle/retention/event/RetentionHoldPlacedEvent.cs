using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public sealed record RetentionHoldPlacedEvent(
    RetentionId RetentionId,
    RetentionReason Reason,
    Timestamp PlacedAt) : DomainEvent;
