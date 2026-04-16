using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public sealed record StreamTerminatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    StreamSessionId SessionId, string Reason, Timestamp TerminatedAt) : DomainEvent;
