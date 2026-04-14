namespace Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;

public sealed record RevenueProcessingIntent(
    Guid RevenueId,
    string SpvId,
    Guid VaultAccountId,
    decimal Amount,
    string Currency,
    string SourceRef);
