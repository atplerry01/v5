namespace Whycespace.Shared.Contracts.Events.Economic.Revenue.Distribution;

public sealed record DistributionCreatedEventSchema(
    Guid AggregateId,
    string SpvId,
    decimal TotalAmount);

/// <summary>
/// Phase 7 B1 — compensation-requested wire shape. Mirrors
/// <c>DistributionCompensationRequestedEvent</c> 1:1. OriginalPayoutId is
/// the correlation anchor that ties the terminal distribution compensation
/// to the payout whose reversal drove it.
/// </summary>
public sealed record DistributionCompensationRequestedEventSchema(
    Guid AggregateId,
    string OriginalPayoutId,
    string Reason,
    DateTimeOffset RequestedAt);

/// <summary>
/// Phase 7 B1 — terminal compensated wire shape. CompensatingJournalId
/// is the ledger journal id produced by the sibling PayoutCompensationWorkflow.
/// </summary>
public sealed record DistributionCompensatedEventSchema(
    Guid AggregateId,
    string OriginalPayoutId,
    string CompensatingJournalId,
    DateTimeOffset CompensatedAt);
