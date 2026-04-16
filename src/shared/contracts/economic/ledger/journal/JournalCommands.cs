using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Ledger.Journal;

public sealed record JournalEntryInput(
    Guid EntryId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Direction);

public sealed record PostJournalEntriesCommand(
    Guid LedgerId,
    Guid JournalId,
    IReadOnlyList<JournalEntryInput> Entries) : IHasAggregateId
{
    public Guid AggregateId => LedgerId;
}
