using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public sealed record FeedItemPinnedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    FeedId FeedId, string ItemRef, Timestamp PinnedAt) : DomainEvent;
