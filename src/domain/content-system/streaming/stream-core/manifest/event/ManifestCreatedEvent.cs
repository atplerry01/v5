using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestCreatedEvent(
    ManifestId ManifestId,
    ManifestSourceRef SourceRef,
    ManifestVersion InitialVersion,
    Timestamp CreatedAt) : DomainEvent;
