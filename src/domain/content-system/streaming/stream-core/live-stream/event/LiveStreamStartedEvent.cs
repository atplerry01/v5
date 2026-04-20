using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed record LiveStreamStartedEvent(
    LiveStreamId LiveStreamId,
    Timestamp StartedAt) : DomainEvent;
