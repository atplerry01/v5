using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record JournalEntryPostedEvent(
    Guid TransactionId,
    IReadOnlyList<LedgerEntry> Entries,
    DateTimeOffset Timestamp) : DomainEvent;
