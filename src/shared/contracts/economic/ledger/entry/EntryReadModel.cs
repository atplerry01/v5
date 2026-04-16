namespace Whycespace.Shared.Contracts.Economic.Ledger.Entry;

public sealed record EntryReadModel
{
    public Guid EntryId { get; init; }
    public Guid JournalId { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
}
