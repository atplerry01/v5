namespace Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;

/// <summary>
/// Payload for starting <see cref="PayoutCompensationWorkflowNames.Compensate"/>.
///
/// Correlation discipline (Phase 7 B2):
/// - <see cref="PayoutId"/> pins the aggregate whose Executed|Failed state
///   is being reversed. Must match the originally executed payout id so
///   the compensation workflow converges on the same PayoutAggregate.
/// - <see cref="OriginalJournalId"/> is the id of the ledger journal posted
///   by the payout execution workflow (T3.5). The compensating journal
///   references it so reversal and forward posting remain paired in the
///   ledger stream (T7.4 append-only invariant).
/// - <see cref="DistributionId"/> pins the sibling distribution whose
///   terminal compensation transition the workflow chains into via
///   DistributionCompensationWorkflow.
/// - <see cref="Shares"/> preserves the original participant shares so
///   the compensating journal's debit/credit sign reversal is exact —
///   no recomputation from distribution state (which can drift).
/// </summary>
public sealed record PayoutCompensationIntent(
    Guid PayoutId,
    Guid DistributionId,
    Guid ContractId,
    string SpvId,
    Guid SpvVaultId,
    Guid OriginalJournalId,
    string IdempotencyKey,
    string Reason,
    IReadOnlyList<ParticipantPayoutEntry> Shares);
