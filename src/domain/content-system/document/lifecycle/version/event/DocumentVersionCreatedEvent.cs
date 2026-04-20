using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public sealed record DocumentVersionCreatedEvent(
    DocumentVersionId VersionId,
    DocumentRef DocumentRef,
    VersionNumber VersionNumber,
    ArtifactRef ArtifactRef,
    DocumentVersionId? PreviousVersionId,
    Timestamp CreatedAt) : DomainEvent;
