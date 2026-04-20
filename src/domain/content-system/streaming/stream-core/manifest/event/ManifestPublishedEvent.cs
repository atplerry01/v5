using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestPublishedEvent(
    ManifestId ManifestId,
    ManifestVersion Version,
    Timestamp PublishedAt) : DomainEvent;
