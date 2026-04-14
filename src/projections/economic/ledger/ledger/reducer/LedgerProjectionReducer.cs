using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;

namespace Whycespace.Projections.Economic.Ledger.Ledger.Reducer;

public static class LedgerProjectionReducer
{
    public static LedgerReadModel Apply(LedgerReadModel state, LedgerUpdatedEventSchema e)
    {
        var journals = new List<Guid>(state.PostedJournalIds);
        if (!journals.Contains(e.JournalId))
            journals.Add(e.JournalId);

        return state with
        {
            LedgerId = e.AggregateId,
            JournalCount = e.JournalCount,
            PostedJournalIds = journals,
            Status = "Active"
        };
    }

    public static LedgerReadModel Apply(LedgerReadModel state, JournalEntryRecordedEventSchema e)
    {
        var existing = state.Balances.ToDictionary(b => b.AccountId, b => b);

        if (!existing.TryGetValue(e.AccountId, out var row))
            row = new LedgerAccountBalanceReadModel { AccountId = e.AccountId };

        row = e.Direction switch
        {
            "Debit" => row with
            {
                DebitTotal = row.DebitTotal + e.Amount,
                NetBalance = (row.DebitTotal + e.Amount) - row.CreditTotal
            },
            "Credit" => row with
            {
                CreditTotal = row.CreditTotal + e.Amount,
                NetBalance = row.DebitTotal - (row.CreditTotal + e.Amount)
            },
            _ => row
        };

        existing[e.AccountId] = row;

        return state with
        {
            Currency = string.IsNullOrEmpty(state.Currency) ? e.Currency : state.Currency,
            Balances = existing.Values.ToList()
        };
    }
}
