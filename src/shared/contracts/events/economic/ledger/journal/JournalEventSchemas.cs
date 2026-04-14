namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;

public sealed record JournalEntryRecordedEventSchema(
    Guid AggregateId,
    Guid EntryId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Direction);

public sealed record JournalPostedEventSchema(
    Guid AggregateId,
    decimal TotalDebit,
    decimal TotalCredit,
    string Currency);
