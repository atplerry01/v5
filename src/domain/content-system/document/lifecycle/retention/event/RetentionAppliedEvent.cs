using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public sealed record RetentionAppliedEvent(
    RetentionId RetentionId,
    RetentionTargetRef TargetRef,
    RetentionWindow Window,
    RetentionReason Reason,
    Timestamp AppliedAt) : DomainEvent;
