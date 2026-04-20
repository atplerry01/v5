using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

public sealed record PlaybackEnabledEvent(
    PlaybackId PlaybackId,
    Timestamp EnabledAt) : DomainEvent;
