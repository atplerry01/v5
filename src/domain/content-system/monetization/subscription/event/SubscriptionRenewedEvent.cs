using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Subscription;

public sealed record SubscriptionRenewedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    SubscriptionId SubscriptionId, Timestamp PeriodStart, Timestamp PeriodEnd, Timestamp RenewedAt) : DomainEvent;
