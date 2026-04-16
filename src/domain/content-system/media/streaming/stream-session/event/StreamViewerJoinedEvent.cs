using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public sealed record StreamViewerJoinedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    StreamSessionId SessionId, string ViewerRef, Timestamp JoinedAt) : DomainEvent;
