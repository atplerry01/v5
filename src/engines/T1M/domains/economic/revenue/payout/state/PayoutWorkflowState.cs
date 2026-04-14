using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;

public static class PayoutExecutionSteps
{
    public const string LoadDistribution = "load_distribution";
    public const string ExecutePayout = "execute_payout";
}

public sealed class PayoutWorkflowState
{
    public Guid PayoutId { get; init; }
    public Guid DistributionId { get; init; }
    public string SpvId { get; init; } = string.Empty;
    public Guid SpvVaultId { get; init; }
    public IReadOnlyList<ParticipantPayoutEntry> Shares { get; init; } = Array.Empty<ParticipantPayoutEntry>();
    public string CurrentStep { get; set; } = string.Empty;
}
