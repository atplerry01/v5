using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

public sealed record PlaybackCreatedEvent(
    PlaybackId PlaybackId,
    PlaybackSourceRef SourceRef,
    PlaybackMode Mode,
    PlaybackWindow Window,
    Timestamp CreatedAt) : DomainEvent;
