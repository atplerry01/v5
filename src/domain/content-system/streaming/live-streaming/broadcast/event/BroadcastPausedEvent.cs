using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastPausedEvent(
    BroadcastId BroadcastId,
    Timestamp PausedAt) : DomainEvent;
