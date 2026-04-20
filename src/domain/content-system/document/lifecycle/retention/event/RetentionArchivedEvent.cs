using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public sealed record RetentionArchivedEvent(
    RetentionId RetentionId,
    Timestamp ArchivedAt) : DomainEvent;
