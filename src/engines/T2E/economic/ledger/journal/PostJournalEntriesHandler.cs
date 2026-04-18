using Whycespace.Domain.EconomicSystem.Ledger.Entry;
using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Ledger.Journal;

public sealed class PostJournalEntriesHandler : IEngine
{
    private readonly IClock _clock;

    public PostJournalEntriesHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PostJournalEntriesCommand cmd)
            return;

        // Phase 4.5 T4.5.3 — control-plane origin gate. Both legitimate
        // ledger paths set CommandContext.IsSystem = true via
        // ISystemIntentDispatcher.DispatchSystemAsync:
        //   * Transaction lifecycle: PostToLedgerStep
        //   * Revenue payout pipeline: PostLedgerJournalStep
        // Any caller using DispatchAsync (the public, user-originated path)
        // observes IsSystem == false and is rejected here. This closes the
        // bypass identified in Phase 4 Finding 12 at the engine boundary,
        // independent of any API-side restriction.
        if (!context.IsSystem)
            throw JournalErrors.ControlPlaneOriginRequired();

        var now = new Timestamp(_clock.UtcNow);
        var emitted = new List<object>();

        var journal = JournalAggregate.Create(new JournalId(cmd.JournalId), now);
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

        // Per-entry emission: every posted journal entry produces a corresponding
        // LedgerEntryRecordedEvent so the entry projection pipeline populates.
        // EntryIds are deterministic (carried from the command), so replay yields
        // identical aggregate ids and the entry_read_model upsert stays idempotent.
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

        // T1.4 — Compute balanced totals from the command's entries and pass
        // them to the ledger so the ledger aggregate can enforce its own
        // balance invariant (per-journal and cumulative).
        var totalDebit = new Amount(cmd.Entries
            .Where(e => e.Direction == "Debit")
            .Sum(e => e.Amount));
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
