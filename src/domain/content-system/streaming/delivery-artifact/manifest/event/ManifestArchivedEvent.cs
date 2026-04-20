using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Manifest;

public sealed record ManifestArchivedEvent(
    ManifestId ManifestId,
    Timestamp ArchivedAt) : DomainEvent;
