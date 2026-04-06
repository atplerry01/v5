namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public enum EntryType
{
    Debit,
    Credit
}

public sealed class LedgerEntry : Entity
{
    public Guid AccountId { get; private set; }
    public Amount Amount { get; private set; }
    public EntryType EntryType { get; private set; }
    public Guid TransactionId { get; private set; }

    private LedgerEntry() { }

    public static LedgerEntry Create(
        LedgerEntryId entryId,
        Guid accountId,
        Amount amount,
        EntryType entryType,
        Guid transactionId)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId cannot be empty.", nameof(accountId));
        if (amount.IsZero || amount.IsNegative)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        if (transactionId == Guid.Empty)
            throw new ArgumentException("TransactionId cannot be empty. Entry must be linked to a transaction.", nameof(transactionId));

        return new LedgerEntry
        {
            Id = entryId.Value,
            AccountId = accountId,
            Amount = amount,
            EntryType = entryType,
            TransactionId = transactionId
        };
    }
}
