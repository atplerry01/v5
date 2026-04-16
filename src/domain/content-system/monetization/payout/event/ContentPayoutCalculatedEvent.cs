using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public sealed record ContentPayoutCalculatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ContentPayoutId PayoutId, string ContentRef, decimal GrossAmount, string CurrencyCode, Timestamp CalculatedAt) : DomainEvent;
