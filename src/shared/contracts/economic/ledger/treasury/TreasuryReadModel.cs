namespace Whycespace.Shared.Contracts.Economic.Ledger.Treasury;

public sealed record TreasuryReadModel
{
    public Guid TreasuryId { get; init; }
    public decimal Balance { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
