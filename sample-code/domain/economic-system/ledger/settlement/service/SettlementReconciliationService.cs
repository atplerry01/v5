using Whycespace.Domain.EconomicSystem.Ledger.Ledger;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementReconciliationService
{
    public bool MatchesLedger(
        SettlementCompletedEvent settlement,
        IReadOnlyList<LedgerEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(settlement);
        ArgumentNullException.ThrowIfNull(entries);

        if (entries.Count == 0)
            return false;

        var totalDebits = 0m;
        var totalCredits = 0m;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (entry.EntryType == EntryType.Debit)
                totalDebits += entry.Amount.Value;
            else
                totalCredits += entry.Amount.Value;
        }

        return totalDebits == totalCredits && settlement.Amount > 0;
    }
}
