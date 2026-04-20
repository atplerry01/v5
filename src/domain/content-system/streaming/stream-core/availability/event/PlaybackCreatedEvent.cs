using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackCreatedEvent(
    PlaybackId PlaybackId,
    PlaybackSourceRef SourceRef,
    PlaybackMode Mode,
    PlaybackWindow Window,
    Timestamp CreatedAt) : DomainEvent;
