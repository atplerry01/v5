using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public sealed record FeedItemAppendedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    FeedId FeedId, string ItemRef, int Rank, Timestamp AppendedAt) : DomainEvent;
