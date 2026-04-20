using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestArchivedEvent(
    ManifestId ManifestId,
    Timestamp ArchivedAt) : DomainEvent;
