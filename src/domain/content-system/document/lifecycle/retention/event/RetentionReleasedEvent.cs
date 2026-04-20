using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public sealed record RetentionReleasedEvent(
    RetentionId RetentionId,
    Timestamp ReleasedAt) : DomainEvent;
