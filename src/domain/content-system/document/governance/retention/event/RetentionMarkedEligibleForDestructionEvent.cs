using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionMarkedEligibleForDestructionEvent(
    RetentionId RetentionId,
    Timestamp MarkedAt) : DomainEvent;
