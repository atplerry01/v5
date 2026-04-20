using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed record LiveStreamCancelledEvent(
    LiveStreamId LiveStreamId,
    string Reason,
    Timestamp CancelledAt) : DomainEvent;
