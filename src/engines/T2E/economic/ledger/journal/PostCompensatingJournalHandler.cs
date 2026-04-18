using DomainCompensationReference = Whycespace.Domain.EconomicSystem.Ledger.Journal.CompensationReference;
using Whycespace.Domain.EconomicSystem.Ledger.Entry;
using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Ledger.Journal;

/// <summary>
/// Phase 7 T7.4 — posts an append-only compensating journal that reverses
/// a previously posted original. The original is never mutated; this
/// handler creates a FRESH Kind=Compensating journal aggregate whose
/// <see cref="CompensationReference"/> provably links back to the
/// original on its own event stream.
///
/// Execution gates (in order):
/// 1. Control-plane origin — IsSystem must be true (same as
///    <see cref="PostJournalEntriesHandler"/>: only the
///    PayoutCompensationWorkflow / transaction-lifecycle paths may post).
/// 2. Idempotent replay — if the compensating journal aggregate already
///    has an event history (deterministic <c>JournalId</c>, so retries
///    converge on the same stream) the handler short-circuits to a
///    no-op. This prevents duplicate compensating journals under
///    at-least-once command redelivery.
/// 3. Referential integrity — the original journal must exist, be
///    <see cref="JournalStatus.Posted"/>, and be
///    <see cref="JournalKind.Standard"/>. Compensating a compensating
///    journal (reflexive compensation) is forbidden — if reversal is
///    needed, roll forward with a new standard journal.
/// 4. Reversal value symmetry — the compensating entries' total debit
///    and total credit must each equal the original's posted total.
///    Since both sides must be internally balanced (debit=credit), this
///    reduces to a single-total equality check that is provable at the
///    event-stream level without any projection.
///
/// Aggregate-side invariants (Kind/Compensation coupling, balance at
/// Post, per-entry positivity) are enforced by
/// <see cref="JournalAggregate"/> itself — this handler delegates and
/// does not duplicate that logic.
/// </summary>
public sealed class PostCompensatingJournalHandler : IEngine
{
    private readonly IClock _clock;
    private readonly IEventStore _eventStore;

    public PostCompensatingJournalHandler(IClock clock, IEventStore eventStore)
    {
        _clock = clock;
        _eventStore = eventStore;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PostCompensatingJournalCommand cmd)
            return;

        if (!context.IsSystem)
            throw JournalErrors.ControlPlaneOriginRequired();

        // ── (2) Idempotent replay short-circuit ──────────────────────
        // JournalId is supplied deterministically by
        // PostCompensatingLedgerJournalStep (IIdGenerator over
        // "journal|compensating|{PayoutId:N}"), so a replayed command
        // lands on the same aggregate stream. If that stream already
        // carries the compensating-created event, the compensation has
        // already been posted — emit no events so the dispatcher returns
        // an idempotent success (CommandResult.Success([])).
        var priorJournal = await _eventStore.LoadEventsAsync(cmd.JournalId);
        if (priorJournal.Count > 0)
            return;

        // ── (3) Referential integrity — original journal state ──────
        var originalStream = await _eventStore.LoadEventsAsync(cmd.Compensation.OriginalJournalId);
        if (originalStream.Count == 0)
            throw JournalErrors.OriginalJournalNotFound(cmd.Compensation.OriginalJournalId);

        var originalAggregate = (JournalAggregate)System.Activator.CreateInstance(
            typeof(JournalAggregate), nonPublic: true)!;
        originalAggregate.HydrateIdentity(cmd.Compensation.OriginalJournalId);
        originalAggregate.LoadFromHistory(originalStream);

        if (originalAggregate.Status != JournalStatus.Posted)
            throw JournalErrors.OriginalJournalNotPosted(cmd.Compensation.OriginalJournalId);

        if (originalAggregate.Kind != JournalKind.Standard)
            throw JournalErrors.OriginalJournalAlreadyCompensating(cmd.Compensation.OriginalJournalId);

        // ── (4) Reversal value symmetry ─────────────────────────────
        // Original totals are taken from the replayed aggregate (derived
        // from JournalEntryAddedEvent history, not from a projection).
        var originalDebitTotal = originalAggregate.Entries
            .Where(e => e.Direction == BookingDirection.Debit)
            .Sum(e => e.Amount.Value);

        var compensatingDebitTotal = cmd.Entries
            .Where(e => e.Direction == "Debit")
            .Sum(e => e.Amount);

        if (originalDebitTotal != compensatingDebitTotal)
            throw JournalErrors.CompensationTotalsMismatch(
                cmd.Compensation.OriginalJournalId,
                originalDebitTotal,
                compensatingDebitTotal);

        // ── Journal creation + posting ───────────────────────────────
        var now = new Timestamp(_clock.UtcNow);
        var emitted = new List<object>();

        var domainCompensation = new DomainCompensationReference(
            cmd.Compensation.OriginalJournalId,
            cmd.Compensation.Reason);

        var journal = JournalAggregate.CreateCompensating(
            new JournalId(cmd.JournalId),
            domainCompensation,
            now);
        emitted.AddRange(journal.DomainEvents);
        journal.ClearDomainEvents();

        foreach (var e in cmd.Entries)
        {
            var direction = ParseDirection(e.Direction);
            journal.AddEntry(
                e.EntryId,
                e.AccountId,
                new Amount(e.Amount),
                new Currency(e.Currency),
                direction,
                e.FxRateId,
                e.FxRate);
        }
        emitted.AddRange(journal.DomainEvents);
        journal.ClearDomainEvents();

        journal.Post(now);
        emitted.AddRange(journal.DomainEvents);
        journal.ClearDomainEvents();

        // Per-entry projection emission (mirrors PostJournalEntriesHandler)
        // so the entry read-model picks up compensating entries just like
        // standard entries. Entry ids are deterministic (supplied by the
        // upstream step from PayoutId + participant id), so replay yields
        // identical aggregate ids and entry-side idempotency holds.
        foreach (var e in cmd.Entries)
        {
            var entry = LedgerEntryAggregate.Record(
                new EntryId(e.EntryId),
                cmd.JournalId,
                e.AccountId,
                new Amount(e.Amount),
                new Currency(e.Currency),
                ParseEntryDirection(e.Direction),
                now);
            emitted.AddRange(entry.DomainEvents);
            entry.ClearDomainEvents();
        }

        // LedgerAggregate.AppendJournal enforces its own
        // DuplicateJournal invariant as a second line of defence
        // against any bypass of the journal-side idempotency check
        // above.
        var totalDebit = new Amount(compensatingDebitTotal);
        var totalCredit = new Amount(cmd.Entries
            .Where(e => e.Direction == "Credit")
            .Sum(e => e.Amount));

        var ledger = (LedgerAggregate)await context.LoadAggregateAsync(typeof(LedgerAggregate));
        ledger.AppendJournal(cmd.JournalId, totalDebit, totalCredit, now);
        emitted.AddRange(ledger.DomainEvents);
        ledger.ClearDomainEvents();

        context.EmitEvents(emitted);
    }

    private static BookingDirection ParseDirection(string direction) =>
        direction switch
        {
            "Debit" => BookingDirection.Debit,
            "Credit" => BookingDirection.Credit,
            _ => throw JournalErrors.InvalidDirection()
        };

    private static EntryDirection ParseEntryDirection(string direction) =>
        direction switch
        {
            "Debit" => EntryDirection.Debit,
            "Credit" => EntryDirection.Credit,
            _ => throw JournalErrors.InvalidDirection()
        };
}
