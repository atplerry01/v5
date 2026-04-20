using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed record LiveStreamScheduledEvent(
    LiveStreamId LiveStreamId,
    LiveBroadcastWindow Window,
    Timestamp ScheduledAt) : DomainEvent;
