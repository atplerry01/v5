namespace Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;

public sealed record RevenueProcessingIntent(
    Guid RevenueId,
    Guid ContractId,
    string SpvId,
    Guid VaultAccountId,
    decimal Amount,
    string Currency,
    string SourceRef);
