using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;

public static class DistributionWorkflowSteps
{
    public const string EnsureContractActive = "ensure_contract_active";
    public const string Validate = "validate";
    public const string Create = "create";
    public const string Confirm = "confirm";
    public const string TriggerPayout = "trigger_payout";
}

public sealed class DistributionWorkflowState
{
    public Guid DistributionId { get; init; }
    public Guid ContractId { get; init; }
    public string SpvId { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<DistributionAllocation> Allocations { get; init; } = Array.Empty<DistributionAllocation>();
    public string CurrentStep { get; set; } = string.Empty;
}
