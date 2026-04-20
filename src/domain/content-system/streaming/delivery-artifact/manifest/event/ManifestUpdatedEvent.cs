using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Manifest;

public sealed record ManifestUpdatedEvent(
    ManifestId ManifestId,
    ManifestVersion PreviousVersion,
    ManifestVersion NewVersion,
    Timestamp UpdatedAt) : DomainEvent;
