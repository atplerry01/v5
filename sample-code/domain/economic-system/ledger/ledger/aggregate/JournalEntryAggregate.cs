namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class JournalEntryAggregate : AggregateRoot
{
    public Guid TransactionId { get; private set; }
    public IReadOnlyList<LedgerEntry> Entries { get; private set; } = [];

    public static JournalEntryAggregate CreateJournalEntry(
        Guid transactionId,
        IReadOnlyList<LedgerEntry> entries,
        LedgerInvariantService invariantService,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(invariantService);

        if (transactionId == Guid.Empty)
            throw new ArgumentException("TransactionId cannot be empty.", nameof(transactionId));

        if (entries.Count < 2)
            throw new LedgerImbalanceException("A journal entry requires at least two entries. Single-entry journals are not allowed.");

        var debitCredits = new List<DebitCredit>(entries.Count);
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var (debit, credit) = entry.EntryType == EntryType.Debit
                ? (entry.Amount.Value, 0m)
                : (0m, entry.Amount.Value);
            debitCredits.Add(new DebitCredit(debit, credit));
        }

        var result = invariantService.Validate(debitCredits);

        if (!result.IsValid)
            throw new LedgerImbalanceException(result.Error!);

        var aggregate = new JournalEntryAggregate();
        aggregate.Id = transactionId;
        aggregate.TransactionId = transactionId;
        aggregate.Entries = entries;

        aggregate.RaiseDomainEvent(new JournalEntryPostedEvent(
            transactionId,
            entries,
            timestamp));

        return aggregate;
    }
}
