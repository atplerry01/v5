using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastStartedEvent(
    BroadcastId BroadcastId,
    Timestamp StartedAt) : DomainEvent;
