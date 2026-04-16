using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public sealed record PlaybackStoppedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PlaybackSessionId SessionId, Timestamp StoppedAt) : DomainEvent;
