using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public sealed record PricingPlanDefinedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PricingPlanId PlanId, string Name, Timestamp DefinedAt) : DomainEvent;
