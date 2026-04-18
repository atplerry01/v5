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

    public void AppendJournal(
        Guid journalId,
        Amount totalDebit,
        Amount totalCredit,
        Timestamp appendedAt)
    {
        Guard.Against(journalId == Guid.Empty, LedgerErrors.InvalidJournalReference().Message);

        // T1.4 — Per-journal balance enforcement. Journal-layer invariants
        // already require balanced entries at Post(), but the ledger
        // defends in depth so no unbalanced journal can enter the ledger
        // stream even if an upstream caller were to bypass the journal
        // aggregate.
        if (totalDebit.Value < 0m || totalCredit.Value < 0m)
            throw LedgerErrors.NegativeJournalTotal();

        if (totalDebit.Value != totalCredit.Value)
            throw LedgerErrors.UnbalancedJournal(journalId, totalDebit.Value, totalCredit.Value);

        if (_journals.Any(j => j.JournalId == journalId))
            throw LedgerErrors.DuplicateJournal(journalId);

        RaiseDomainEvent(new JournalAppendedToLedgerEvent(LedgerId, journalId, totalDebit, totalCredit, appendedAt));
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
                _journals.Add(PostedJournalReference.Create(e.JournalId, e.TotalDebit, e.TotalCredit, e.AppendedAt));
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

        // T1.4 — Cumulative ledger balance invariant. Given that every
        // journal is individually balanced, the sum is trivially balanced.
        // Asserting anyway is cheap defense-in-depth: any future code path
        // that mutates _journals outside AppendJournal is caught here.
        var cumulativeDebit = _journals.Sum(j => j.TotalDebit.Value);
        var cumulativeCredit = _journals.Sum(j => j.TotalCredit.Value);
        if (cumulativeDebit != cumulativeCredit)
            throw LedgerErrors.LedgerBalanceInvariantViolation(cumulativeDebit, cumulativeCredit);
    }
}
