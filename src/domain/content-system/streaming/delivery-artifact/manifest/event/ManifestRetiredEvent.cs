using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Manifest;

public sealed record ManifestRetiredEvent(
    ManifestId ManifestId,
    string Reason,
    Timestamp RetiredAt) : DomainEvent;
