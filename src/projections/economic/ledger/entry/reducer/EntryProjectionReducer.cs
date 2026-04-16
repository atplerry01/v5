using Whycespace.Shared.Contracts.Economic.Ledger.Entry;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Entry;

namespace Whycespace.Projections.Economic.Ledger.Entry.Reducer;

public static class EntryProjectionReducer
{
    public static EntryReadModel Apply(EntryReadModel state, LedgerEntryRecordedEventSchema e) =>
        state with
        {
            EntryId = e.AggregateId,
            JournalId = e.JournalId,
            AccountId = e.AccountId,
            Amount = e.Amount,
            Currency = e.Currency,
            Direction = e.Direction,
            RecordedAt = e.RecordedAt
        };
}
