using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Manifest;

public sealed record ManifestPublishedEvent(
    ManifestId ManifestId,
    ManifestVersion Version,
    Timestamp PublishedAt) : DomainEvent;
