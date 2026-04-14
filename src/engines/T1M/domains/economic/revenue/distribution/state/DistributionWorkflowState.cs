using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;

public static class DistributionWorkflowSteps
{
    public const string Validate = "validate";
    public const string Create = "create";
}

public sealed class DistributionWorkflowState
{
    public Guid DistributionId { get; init; }
    public string SpvId { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<DistributionAllocation> Allocations { get; init; } = Array.Empty<DistributionAllocation>();
    public string CurrentStep { get; set; } = string.Empty;
}
