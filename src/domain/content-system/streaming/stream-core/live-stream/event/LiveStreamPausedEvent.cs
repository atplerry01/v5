using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed record LiveStreamPausedEvent(
    LiveStreamId LiveStreamId,
    Timestamp PausedAt) : DomainEvent;
