namespace Whycespace.Shared.Contracts.Economic.Ledger.Ledger;

public sealed record LedgerAccountBalanceReadModel
{
    public Guid AccountId { get; init; }
    public decimal DebitTotal { get; init; }
    public decimal CreditTotal { get; init; }
    public decimal NetBalance { get; init; }
}

public sealed record LedgerReadModel
{
    public Guid LedgerId { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int JournalCount { get; init; }
    public IReadOnlyList<Guid> PostedJournalIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<LedgerAccountBalanceReadModel> Balances { get; init; } = Array.Empty<LedgerAccountBalanceReadModel>();
    public string Status { get; init; } = string.Empty;
}
