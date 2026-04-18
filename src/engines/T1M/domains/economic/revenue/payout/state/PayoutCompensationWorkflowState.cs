using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;

public static class PayoutCompensationSteps
{
    public const string RequestCompensation = "request_payout_compensation";
    public const string PostCompensatingLedgerJournal = "post_compensating_ledger_journal";
    public const string MarkCompensated = "mark_payout_compensated";
}

/// <summary>
/// Phase 7 T7.3 — workflow state for reversing an Executed or Failed payout.
///
/// Canonical orchestration:
///   RequestPayoutCompensationStep
///     -> PostCompensatingLedgerJournalStep (T7.4: append-only reversal)
///     -> MarkPayoutCompensatedStep
/// On success the workflow also signals DistributionCompensationWorkflow
/// so the upstream distribution can terminate in Compensated state.
/// </summary>
public sealed class PayoutCompensationWorkflowState
{
    public Guid PayoutId { get; init; }
    public Guid DistributionId { get; init; }
    public Guid ContractId { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public string SpvId { get; init; } = string.Empty;
    public Guid SpvVaultId { get; init; }
    public IReadOnlyList<ParticipantPayoutEntry> Shares { get; init; } = Array.Empty<ParticipantPayoutEntry>();

    // Links the reversal to the original posting.
    public Guid OriginalJournalId { get; init; }
    public string Reason { get; init; } = string.Empty;

    // Set by PostCompensatingLedgerJournalStep and consumed by MarkCompensated
    // and the sibling DistributionCompensationWorkflow.
    public Guid CompensatingJournalId { get; set; }

    public string CurrentStep { get; set; } = string.Empty;
}
