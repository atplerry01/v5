using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public sealed record DocumentVersionActivatedEvent(
    DocumentVersionId VersionId,
    Timestamp ActivatedAt) : DomainEvent;
