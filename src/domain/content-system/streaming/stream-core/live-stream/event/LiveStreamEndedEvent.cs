using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed record LiveStreamEndedEvent(
    LiveStreamId LiveStreamId,
    Timestamp EndedAt) : DomainEvent;
