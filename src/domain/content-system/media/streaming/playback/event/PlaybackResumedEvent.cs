using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public sealed record PlaybackResumedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PlaybackSessionId SessionId, long PositionMs, Timestamp ResumedAt) : DomainEvent;
