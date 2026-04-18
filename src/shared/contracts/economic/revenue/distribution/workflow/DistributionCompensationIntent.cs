namespace Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;

/// <summary>
/// Payload for starting
/// <see cref="DistributionCompensationWorkflowNames.Compensate"/>.
///
/// Correlation discipline (Phase 7 B2):
/// - <see cref="DistributionId"/> pins the DistributionAggregate whose
///   Paid|Failed state is being terminally compensated.
/// - <see cref="OriginalPayoutId"/> pins the sibling payout whose
///   compensation drove this workflow. The aggregate enforces that this
///   id must match the originally paid payout (T7.2 correlation
///   invariant), preventing cross-distribution compensation drift.
/// - <see cref="CompensatingJournalId"/> is the id of the compensating
///   ledger journal posted by PayoutCompensationWorkflow (T7.4). The
///   distribution aggregate requires it before MarkCompensated can
///   transition — compensation is never "blind".
/// </summary>
public sealed record DistributionCompensationIntent(
    Guid DistributionId,
    Guid OriginalPayoutId,
    string CompensatingJournalId,
    string Reason);
