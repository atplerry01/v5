using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

public sealed record PlaybackDisabledEvent(
    PlaybackId PlaybackId,
    string Reason,
    Timestamp DisabledAt) : DomainEvent;
