namespace Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;

public sealed record DistributionCreationIntent(
    Guid DistributionId,
    Guid ContractId,
    string SpvId,
    decimal TotalAmount,
    IReadOnlyList<DistributionAllocation> Allocations);
