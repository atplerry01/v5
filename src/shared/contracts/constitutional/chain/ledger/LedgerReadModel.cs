namespace Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;

public sealed record LedgerReadModel
{
    public Guid LedgerId { get; init; }
    public string LedgerName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset OpenedAt { get; init; }
    public DateTimeOffset? SealedAt { get; init; }
}
