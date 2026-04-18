namespace Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;

public sealed record PayoutRequestedEventSchema(
    Guid AggregateId,
    Guid DistributionId,
    string IdempotencyKey,
    DateTimeOffset RequestedAt);

/// <summary>
/// V1 wire shape: <c>(AggregateId, DistributionId)</c> — preserved as the
/// positional constructor so messages produced before Phase 3 still
/// deserialize. V2 fields (<c>IdempotencyKey</c>, <c>ExecutedAt</c>) are
/// init-only with defaults; missing JSON keys deserialize to empty / default
/// instead of throwing. Phase 3.5 T3.5.1.
/// </summary>
public sealed record PayoutExecutedEventSchema(
    Guid AggregateId,
    Guid DistributionId)
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; init; }
}

public sealed record PayoutFailedEventSchema(
    Guid AggregateId,
    Guid DistributionId,
    string Reason,
    DateTimeOffset FailedAt);

/// <summary>
/// Phase 7 B2 — compensation-requested wire shape. Mirrors
/// <c>PayoutCompensationRequestedEvent</c> 1:1.
/// </summary>
public sealed record PayoutCompensationRequestedEventSchema(
    Guid AggregateId,
    Guid DistributionId,
    string IdempotencyKey,
    string Reason,
    DateTimeOffset RequestedAt);

/// <summary>
/// Phase 7 B2 — terminal compensated wire shape. Mirrors
/// <c>PayoutCompensatedEvent</c> 1:1.
/// </summary>
public sealed record PayoutCompensatedEventSchema(
    Guid AggregateId,
    Guid DistributionId,
    string IdempotencyKey,
    string CompensatingJournalId,
    DateTimeOffset CompensatedAt);
