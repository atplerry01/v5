using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public sealed record PricingPriceAdjustedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PricingPlanId PlanId, decimal Amount, string CurrencyCode, Timestamp AdjustedAt) : DomainEvent;
