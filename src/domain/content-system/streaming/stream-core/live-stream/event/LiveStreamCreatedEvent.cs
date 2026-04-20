using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed record LiveStreamCreatedEvent(
    LiveStreamId LiveStreamId,
    StreamRef StreamRef,
    Timestamp CreatedAt) : DomainEvent;
