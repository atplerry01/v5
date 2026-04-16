using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public sealed record PricingPlanDeprecatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PricingPlanId PlanId, Timestamp DeprecatedAt) : DomainEvent;
