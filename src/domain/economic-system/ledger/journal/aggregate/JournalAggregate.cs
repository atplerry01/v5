using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

/// <summary>
/// Journal aggregate.
///
/// Phase 7 T7.4 — extended with <see cref="JournalKind"/> +
/// <see cref="CompensationReference"/> so compensating journals carry
/// their provable link back to the original they reverse on the
/// aggregate's own event stream. The ledger is append-only: original
/// journals are never mutated; a reversal is a fresh, balanced journal
/// whose net effect cancels the original.
///
/// Kind/Compensation coupling invariants (EnsureInvariants):
/// - Kind=Compensating ↔ Compensation non-null
/// - Kind=Standard     ↔ Compensation null
/// - OriginalJournalId != JournalId (factory-time check)
///
/// Kind is set at creation by selecting the factory
/// (<see cref="Create"/> → Standard; <see cref="CreateCompensating"/> →
/// Compensating). Once set, Kind is immutable for the lifetime of the
/// aggregate.
/// </summary>
public sealed class JournalAggregate : AggregateRoot
{
    public JournalId JournalId { get; private set; }
    public Currency Currency { get; private set; }
    public JournalStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? PostedAt { get; private set; }

    // Phase 7 T7.4 — compensation state. Defaults to Standard; switched to
    // Compensating exactly once by CreateCompensating via
    // JournalCompensationCreatedEvent.
    public JournalKind Kind { get; private set; } = JournalKind.Standard;
    public CompensationReference? Compensation { get; private set; }

    private readonly List<JournalEntry> _entries = new();
    public IReadOnlyList<JournalEntry> Entries => _entries.AsReadOnly();

    private JournalAggregate() { }

    public static JournalAggregate Create(JournalId journalId, Timestamp createdAt)
    {
        var aggregate = new JournalAggregate();

        aggregate.RaiseDomainEvent(new JournalCreatedEvent(journalId, createdAt));

        return aggregate;
    }

    /// <summary>
    /// Phase 7 T7.4 — creates a Kind=Compensating journal bound to the
    /// original it reverses. Factory-level checks reject an empty or
    /// self-referential compensation reference up front; the emitted
    /// <see cref="JournalCompensationCreatedEvent"/> stamps the link onto
    /// the aggregate stream so the ledger-level referential integrity is
    /// replay-safe and audit-reconstructable.
    /// </summary>
    public static JournalAggregate CreateCompensating(
        JournalId journalId,
        CompensationReference compensation,
        Timestamp createdAt)
    {
        if (compensation is null)
            throw JournalErrors.CompensationReferenceRequired();

        if (compensation.OriginalJournalId == journalId.Value)
            throw JournalErrors.CompensatingJournalCannotReferenceSelf();

        var aggregate = new JournalAggregate();

        aggregate.RaiseDomainEvent(new JournalCompensationCreatedEvent(journalId, compensation, createdAt));

        return aggregate;
    }

    public void AddEntry(
        Guid entryId,
        Guid accountId,
        Amount amount,
        Currency currency,
        BookingDirection direction,
        Guid? fxRateId = null,
        decimal? fxRate = null)
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

        // Phase 6 T6.1 — FX rate snapshot must arrive as a paired
        // (RateId, Rate) tuple. Enforcing both-or-neither keeps the ledger
        // read path unambiguous: either there is a full deterministic
        // snapshot or there is none.
        Guard.Against(
            fxRateId.HasValue != fxRate.HasValue,
            "FX rate snapshot must carry both FxRateId and FxRate, or neither.");
        Guard.Against(
            fxRate.HasValue && fxRate.Value <= 0m,
            "FxRate must be positive when provided.");

        RaiseDomainEvent(new JournalEntryAddedEvent(
            JournalId,
            entryId,
            accountId,
            amount,
            currency,
            direction,
            fxRateId,
            fxRate));
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
                // Kind stays at its initial Standard default; Compensation
                // remains null. The invariant check in EnsureInvariants
                // guarantees this coupling holds after Apply.
                break;

            case JournalCompensationCreatedEvent e:
                JournalId = e.JournalId;
                Status = JournalStatus.Open;
                CreatedAt = e.CreatedAt;
                Kind = JournalKind.Compensating;
                Compensation = e.Compensation;
                break;

            case JournalEntryAddedEvent e:
                var entry = JournalEntry.Create(
                    e.EntryId,
                    e.AccountId,
                    e.Amount,
                    e.Currency,
                    e.Direction,
                    e.FxRateId,
                    e.FxRate);
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

        // Phase 7 T7.4 — Kind/Compensation coupling. Runs after every
        // Apply so any bypass path that attempts to set one without the
        // other is rejected before the event is appended.
        if (Kind == JournalKind.Compensating && Compensation is null)
            throw JournalErrors.CompensationMissingForCompensatingKind();
        if (Kind == JournalKind.Standard && Compensation is not null)
            throw JournalErrors.CompensationSetForStandardKind();

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
