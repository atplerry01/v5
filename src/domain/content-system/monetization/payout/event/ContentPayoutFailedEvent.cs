using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public sealed record ContentPayoutFailedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ContentPayoutId PayoutId, string Reason, Timestamp FailedAt) : DomainEvent;
