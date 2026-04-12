using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed class JournalAggregate : AggregateRoot
{
    public JournalId JournalId { get; private set; }
    public Currency Currency { get; private set; }
    public JournalStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? PostedAt { get; private set; }

    private readonly List<JournalEntry> _entries = new();
    public IReadOnlyList<JournalEntry> Entries => _entries.AsReadOnly();

    private JournalAggregate() { }

    public static JournalAggregate Create(JournalId journalId, Timestamp createdAt)
    {
        var aggregate = new JournalAggregate();

        aggregate.RaiseDomainEvent(new JournalCreatedEvent(journalId, createdAt));

        return aggregate;
    }

    public void AddEntry(
        Guid entryId,
        Guid accountId,
        Amount amount,
        Currency currency,
        BookingDirection direction)
    {
        Guard.Against(Status == JournalStatus.Posted, JournalErrors.JournalAlreadyPosted().Message);
        Guard.Against(amount.Value <= 0, JournalErrors.InvalidAmount().Message);
        Guard.Against(entryId == Guid.Empty, JournalErrors.MissingEntryReference().Message);
        Guard.Against(accountId == Guid.Empty, JournalErrors.MissingAccountReference().Message);
        Guard.Against(
            direction != BookingDirection.Debit && direction != BookingDirection.Credit,
            JournalErrors.InvalidDirection().Message);

        if (_entries.Count > 0)
        {
            Guard.Against(
                Currency.Code != currency.Code,
                JournalErrors.CurrencyMismatch(Currency, currency).Message);
        }

        RaiseDomainEvent(new JournalEntryAddedEvent(
            JournalId,
            entryId,
            accountId,
            amount,
            currency,
            direction));
    }

    public void Post(Timestamp postedAt)
    {
        Guard.Against(Status == JournalStatus.Posted, JournalErrors.JournalAlreadyPosted().Message);
        Guard.Against(_entries.Count < 2, JournalErrors.InsufficientEntries(_entries.Count).Message);

        var totalDebit = CalculateTotalDebit();
        var totalCredit = CalculateTotalCredit();

        Guard.Against(
            totalDebit.Value != totalCredit.Value,
            JournalErrors.UnbalancedJournal(totalDebit, totalCredit).Message);

        RaiseDomainEvent(new JournalPostedEvent(JournalId, totalDebit, totalCredit, postedAt));
    }

    protected override void Apply(object @event)
    {
        switch (@event)
        {
            case JournalCreatedEvent e:
                JournalId = e.JournalId;
                Status = JournalStatus.Open;
                CreatedAt = e.CreatedAt;
                break;

            case JournalEntryAddedEvent e:
                var entry = JournalEntry.Create(
                    e.EntryId,
                    e.AccountId,
                    e.Amount,
                    e.Currency,
                    e.Direction);
                _entries.Add(entry);

                if (_entries.Count == 1)
                {
                    Currency = e.Currency;
                }
                break;

            case JournalPostedEvent e:
                Status = JournalStatus.Posted;
                PostedAt = e.PostedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        foreach (var entry in _entries)
        {
            Guard.Against(entry.Amount.Value <= 0, JournalErrors.NegativeEntryAmount().Message);
        }

        if (Status == JournalStatus.Posted)
        {
            var totalDebit = CalculateTotalDebit();
            var totalCredit = CalculateTotalCredit();
            Guard.Against(
                totalDebit.Value != totalCredit.Value,
                JournalErrors.JournalBalanceInvariantViolation(totalDebit, totalCredit).Message);
        }
    }

    private Amount CalculateTotalDebit()
    {
        var total = _entries
            .Where(e => e.Direction == BookingDirection.Debit)
            .Sum(e => e.Amount.Value);
        return new Amount(total);
    }

    private Amount CalculateTotalCredit()
    {
        var total = _entries
            .Where(e => e.Direction == BookingDirection.Credit)
            .Sum(e => e.Amount.Value);
        return new Amount(total);
    }
}
