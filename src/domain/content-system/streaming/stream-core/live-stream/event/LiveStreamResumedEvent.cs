using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed record LiveStreamResumedEvent(
    LiveStreamId LiveStreamId,
    Timestamp ResumedAt) : DomainEvent;
