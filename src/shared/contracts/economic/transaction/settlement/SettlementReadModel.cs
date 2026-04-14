namespace Whycespace.Shared.Contracts.Economic.Transaction.Settlement;

public sealed record SettlementReadModel
{
    public Guid SettlementId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string SourceReference { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ExternalReferenceId { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
}
