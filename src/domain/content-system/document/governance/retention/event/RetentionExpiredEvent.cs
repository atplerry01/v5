using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

public sealed record RetentionExpiredEvent(
    RetentionId RetentionId,
    Timestamp ExpiredAt) : DomainEvent;
