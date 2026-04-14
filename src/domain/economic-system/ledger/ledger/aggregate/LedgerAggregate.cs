using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class LedgerAggregate : AggregateRoot
{
    public LedgerId LedgerId { get; private set; }
    public Currency Currency { get; private set; }
    public Timestamp OpenedAt { get; private set; }

    private readonly List<PostedJournalReference> _journals = new();
    public IReadOnlyList<PostedJournalReference> Journals => _journals.AsReadOnly();

    public static LedgerAggregate Open(LedgerId ledgerId, Currency currency, Timestamp openedAt)
    {
        var ledger = new LedgerAggregate();
        ledger.RaiseDomainEvent(new LedgerOpenedEvent(ledgerId, currency, openedAt));
        return ledger;
    }

    public void AppendJournal(Guid journalId, Timestamp appendedAt)
    {
        Guard.Against(journalId == Guid.Empty, LedgerErrors.InvalidJournalReference().Message);

        if (_journals.Any(j => j.JournalId == journalId))
            throw LedgerErrors.DuplicateJournal(journalId);

        RaiseDomainEvent(new JournalAppendedToLedgerEvent(LedgerId, journalId, appendedAt));
        RaiseDomainEvent(new LedgerUpdatedEvent(LedgerId, journalId, _journals.Count, appendedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LedgerOpenedEvent e:
                LedgerId = e.LedgerId;
                Currency = e.Currency;
                OpenedAt = e.OpenedAt;
                break;

            case JournalAppendedToLedgerEvent e:
                _journals.Add(PostedJournalReference.Create(e.JournalId, e.AppendedAt));
                break;

            case LedgerUpdatedEvent:
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        var duplicateJournalIds = _journals
            .GroupBy(j => j.JournalId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateJournalIds.Count > 0)
            throw LedgerErrors.DuplicateJournal(duplicateJournalIds.First());
    }
}
