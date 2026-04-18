namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.State;

public static class RevenueProcessingSteps
{
    public const string EnsureContractActive = "ensure_contract_active";
    public const string Validate = "validate";
    public const string RecordRevenue = "record_revenue";
    public const string ApplyRevenue = "apply_revenue";
    public const string TriggerDistribution = "trigger_distribution";
}

public sealed class RevenueWorkflowState
{
    public Guid RevenueId { get; init; }
    public Guid ContractId { get; init; }
    public string SpvId { get; init; } = string.Empty;
    public Guid VaultAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
}
