using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public sealed record SubscriptionCancelledEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    SubscriptionId SubscriptionId, string Reason, Timestamp CancelledAt) : DomainEvent;
