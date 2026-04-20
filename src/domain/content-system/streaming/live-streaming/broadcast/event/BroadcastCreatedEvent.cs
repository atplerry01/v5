using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastCreatedEvent(
    BroadcastId BroadcastId,
    StreamRef StreamRef,
    Timestamp CreatedAt) : DomainEvent;
