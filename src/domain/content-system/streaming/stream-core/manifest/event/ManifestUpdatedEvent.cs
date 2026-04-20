using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestUpdatedEvent(
    ManifestId ManifestId,
    ManifestVersion PreviousVersion,
    ManifestVersion NewVersion,
    Timestamp UpdatedAt) : DomainEvent;
