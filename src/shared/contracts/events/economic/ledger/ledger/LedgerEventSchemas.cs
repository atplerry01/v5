namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;

public sealed record LedgerUpdatedEventSchema(
    Guid AggregateId,
    Guid JournalId,
    int JournalCount);
