using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public sealed record ManifestRetiredEvent(
    ManifestId ManifestId,
    string Reason,
    Timestamp RetiredAt) : DomainEvent;
