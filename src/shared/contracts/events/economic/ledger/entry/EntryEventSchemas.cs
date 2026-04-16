namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Entry;

public sealed record LedgerEntryRecordedEventSchema(
    Guid AggregateId,
    Guid JournalId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Direction,
    DateTimeOffset RecordedAt);
