using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;

public static class PayoutExecutionSteps
{
    public const string EnsureContractActive = "ensure_contract_active";
    public const string LoadDistribution = "load_distribution";
    public const string RequestPayout = "request_payout";
    public const string ExecutePayout = "execute_payout";
    public const string MarkPayoutExecuted = "mark_payout_executed";
    public const string PostLedgerJournal = "post_ledger_journal";
    public const string MarkDistributionPaid = "mark_distribution_paid";
}

public sealed class PayoutWorkflowState
{
    public Guid PayoutId { get; init; }
    public Guid DistributionId { get; init; }
    public Guid ContractId { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public string SpvId { get; init; } = string.Empty;
    public Guid SpvVaultId { get; init; }
    public IReadOnlyList<ParticipantPayoutEntry> Shares { get; init; } = Array.Empty<ParticipantPayoutEntry>();
    public string CurrentStep { get; set; } = string.Empty;
}
