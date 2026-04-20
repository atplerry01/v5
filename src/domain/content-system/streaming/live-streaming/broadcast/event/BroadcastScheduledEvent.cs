using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastScheduledEvent(
    BroadcastId BroadcastId,
    BroadcastWindow Window,
    Timestamp ScheduledAt) : DomainEvent;
