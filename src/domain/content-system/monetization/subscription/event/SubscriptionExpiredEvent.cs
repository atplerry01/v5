using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public sealed record SubscriptionExpiredEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    SubscriptionId SubscriptionId, Timestamp ExpiredAt) : DomainEvent;
