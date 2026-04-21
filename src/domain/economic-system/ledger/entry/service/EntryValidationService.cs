namespace Whycespace.Domain.EconomicSystem.Ledger.Entry;

public sealed class EntryValidationService
{
    public bool ValidateEntryIntegrity(LedgerEntryAggregate entry)
    {
        if (entry.Amount.Value <= 0)
            return false;

        if (entry.JournalId.Value == Guid.Empty)
            return false;

        if (entry.AccountId.Value == Guid.Empty)
            return false;

        if (entry.Direction != EntryDirection.Debit && entry.Direction != EntryDirection.Credit)
            return false;

        return true;
    }
}
