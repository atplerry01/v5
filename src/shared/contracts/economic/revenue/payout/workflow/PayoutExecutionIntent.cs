namespace Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;

public sealed record ParticipantPayoutEntry(
    string ParticipantId,
    Guid ParticipantVaultId,
    decimal Amount,
    decimal Percentage);

public sealed record PayoutExecutionIntent(
    Guid PayoutId,
    Guid DistributionId,
    string SpvId,
    Guid SpvVaultId,
    IReadOnlyList<ParticipantPayoutEntry> Shares);
