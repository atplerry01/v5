using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class LedgerReconstructionService
{
    /// <summary>
    /// Represents a single entry for reconstruction purposes.
    /// In production, this would be sourced from the event store.
    /// </summary>
    public readonly record struct ReconstructionEntry(
        Guid AccountId,
        Amount Amount,
        bool IsDebit);

    /// <summary>
    /// Reconstructs account balances from a flat list of entries.
    /// Returns a list of LedgerAccountBalance value objects.
    /// </summary>
    public IReadOnlyList<LedgerAccountBalance> ReconstructBalances(
        IReadOnlyList<ReconstructionEntry> entries)
    {
        var balances = new Dictionary<Guid, (decimal Debits, decimal Credits)>();

        foreach (var entry in entries)
        {
            if (!balances.ContainsKey(entry.AccountId))
                balances[entry.AccountId] = (0m, 0m);

            var current = balances[entry.AccountId];
            if (entry.IsDebit)
                balances[entry.AccountId] = (current.Debits + entry.Amount.Value, current.Credits);
            else
                balances[entry.AccountId] = (current.Debits, current.Credits + entry.Amount.Value);
        }

        var result = new List<LedgerAccountBalance>();
        foreach (var kvp in balances)
        {
            var debitTotal = new Amount(kvp.Value.Debits);
            var creditTotal = new Amount(kvp.Value.Credits);
            var net = new Amount(kvp.Value.Debits - kvp.Value.Credits);
            result.Add(new LedgerAccountBalance(kvp.Key, debitTotal, creditTotal, net));
        }

        return result.AsReadOnly();
    }
}
