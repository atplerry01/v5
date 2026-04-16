using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public sealed record ContentPayoutApprovedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ContentPayoutId PayoutId, string ApproverRef, Timestamp ApprovedAt) : DomainEvent;
