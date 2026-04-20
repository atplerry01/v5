using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackArchivedEvent(
    PlaybackId PlaybackId,
    Timestamp ArchivedAt) : DomainEvent;
