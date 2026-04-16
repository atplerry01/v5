using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public sealed record SubscriptionCreatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    SubscriptionId SubscriptionId, string SubscriberRef, string PlanRef, BillingCycle Cycle, Timestamp CreatedAt) : DomainEvent;
