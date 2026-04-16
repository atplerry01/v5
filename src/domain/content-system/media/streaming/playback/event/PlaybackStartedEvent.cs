using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public sealed record PlaybackStartedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PlaybackSessionId SessionId, string AssetRef, string ViewerRef, Timestamp StartedAt) : DomainEvent;
