namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class LedgerAggregate : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string EntryType { get; private set; } = string.Empty;

    public static LedgerAggregate Record(Guid id, Guid accountId, decimal amount, string entryType)
    {
        var aggregate = new LedgerAggregate();
        aggregate.Id = id;
        aggregate.AccountId = accountId;
        aggregate.Amount = amount;
        aggregate.EntryType = entryType;
        aggregate.RaiseDomainEvent(new LedgerEntryRecordedEvent(id, accountId, amount, entryType));
        return aggregate;
    }

    public JournalEntryAggregate RecordEntries(
        Guid transactionId,
        IReadOnlyList<LedgerEntry> entries,
        LedgerInvariantService invariantService,
        DateTimeOffset timestamp)
    {
        var journal = JournalEntryAggregate.CreateJournalEntry(transactionId, entries, invariantService, timestamp);

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            RaiseDomainEvent(new LedgerEntryRecordedEvent(
                Id, entry.AccountId, entry.Amount.Value,
                entry.EntryType == Ledger.EntryType.Debit ? "Debit" : "Credit"));
        }

        return journal;
    }

    public void RecordDoubleEntry(Guid entryId, LedgerAccount account, DebitAmount debit, CreditAmount credit, Currency currency)
    {
        EnsureInvariant(entryId != Guid.Empty, "EntryId", "EntryId cannot be empty.");
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(currency);
        EnsureInvariant(!debit.Value.IsZero || !credit.Value.IsZero, "NonZeroEntry", "At least one of debit or credit must be non-zero.");

        var balancedSpec = new BalancedLedgerSpecification();
        var entries = new List<DebitCredit> { new(debit.Value.Value, credit.Value.Value) };
        EnsureInvariant(balancedSpec.IsSatisfiedBy(entries), "BalancedLedger", "Double entry must be balanced: total debits must equal total credits.");

        RaiseDomainEvent(new LedgerEntryRecordedEvent(
            Id, entryId, debit.Value.Value, "Debit", currency.Code));
        RaiseDomainEvent(new LedgerEntryRecordedEvent(
            Id, entryId, credit.Value.Value, "Credit", currency.Code));
    }
}
