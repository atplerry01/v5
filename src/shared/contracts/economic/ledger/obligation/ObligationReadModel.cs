namespace Whycespace.Shared.Contracts.Economic.Ledger.Obligation;

public sealed record ObligationReadModel
{
    public Guid ObligationId { get; init; }
    public Guid CounterpartyId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid SettlementId { get; init; }
    public string CancellationReason { get; init; } = string.Empty;
}
