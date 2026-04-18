namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;

public static class DistributionCompensationSteps
{
    public const string RequestCompensation = "request_distribution_compensation";
    public const string MarkCompensated = "mark_distribution_compensated";
}

/// <summary>
/// Phase 7 T7.3 — workflow state for reversing a Paid or Failed distribution.
/// Compensation is driven by PayoutCompensationWorkflow; this workflow
/// reflects the terminal distribution-side transition once the compensating
/// ledger journal has been posted.
/// </summary>
public sealed class DistributionCompensationWorkflowState
{
    public Guid DistributionId { get; init; }
    public Guid OriginalPayoutId { get; init; }
    public string CompensatingJournalId { get; set; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
}
