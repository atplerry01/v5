using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastResumedEvent(
    BroadcastId BroadcastId,
    Timestamp ResumedAt) : DomainEvent;
