using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Playback;

public sealed record PlaybackArchivedEvent(
    PlaybackId PlaybackId,
    Timestamp ArchivedAt) : DomainEvent;
