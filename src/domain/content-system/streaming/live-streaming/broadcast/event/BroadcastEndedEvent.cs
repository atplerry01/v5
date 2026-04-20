using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastEndedEvent(
    BroadcastId BroadcastId,
    Timestamp EndedAt) : DomainEvent;
