using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastCancelledEvent(
    BroadcastId BroadcastId,
    string Reason,
    Timestamp CancelledAt) : DomainEvent;
