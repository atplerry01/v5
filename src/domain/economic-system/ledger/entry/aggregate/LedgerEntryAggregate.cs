using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Entry;

public sealed class LedgerEntryAggregate : AggregateRoot
{
    public EntryId EntryId { get; private set; }
    public Guid JournalId { get; private set; }
    public Guid AccountId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public EntryDirection Direction { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public bool IsRecorded { get; private set; }

    private LedgerEntryAggregate() { }

    public static LedgerEntryAggregate Record(
        EntryId entryId,
        Guid journalId,
        Guid accountId,
        Amount amount,
        Currency currency,
        EntryDirection direction,
        Timestamp createdAt)
    {
        Guard.Against(amount.Value <= 0, EntryErrors.InvalidAmount().Message);
        Guard.Against(journalId == Guid.Empty, EntryErrors.MissingJournalReference().Message);
        Guard.Against(accountId == Guid.Empty, EntryErrors.MissingAccountReference().Message);
        Guard.Against(
            direction != EntryDirection.Debit && direction != EntryDirection.Credit,
            EntryErrors.InvalidDirection().Message);

        var aggregate = new LedgerEntryAggregate();

        aggregate.RaiseDomainEvent(new LedgerEntryRecordedEvent(
            entryId,
            journalId,
            accountId,
            amount,
            currency,
            direction,
            createdAt));

        return aggregate;
    }

    protected override void Apply(object @event)
    {
        switch (@event)
        {
            case LedgerEntryRecordedEvent e:
                EntryId = e.EntryId;
                JournalId = e.JournalId;
                AccountId = e.AccountId;
                Amount = e.Amount;
                Currency = e.Currency;
                Direction = e.Direction;
                CreatedAt = e.CreatedAt;
                IsRecorded = true;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Amount.Value <= 0, EntryErrors.NegativeAmount().Message);
    }
}
