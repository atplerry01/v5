namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;

public sealed record JournalAppendedToLedgerEventSchema(
    Guid AggregateId,
    Guid JournalId,
    decimal TotalDebit,
    decimal TotalCredit,
    DateTimeOffset AppendedAt);