using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public sealed record RetentionExpiredEvent(
    RetentionId RetentionId,
    Timestamp ExpiredAt) : DomainEvent;
