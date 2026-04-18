using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Ledger.Journal;

public sealed record JournalEntryInput(
    Guid EntryId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Direction,
    // Phase 6 T6.1 — optional FX rate snapshot. When a transaction crossed
    // the FX lock step, PostToLedgerStep carries the locked RateId and rate
    // value onto every journal entry so the ledger read path never has to
    // resolve exchange-rate state at query time.
    Guid? FxRateId = null,
    decimal? FxRate = null);

public sealed record PostJournalEntriesCommand(
    Guid LedgerId,
    Guid JournalId,
    IReadOnlyList<JournalEntryInput> Entries) : IHasAggregateId
{
    public Guid AggregateId => LedgerId;
}

// Phase 7 T7.4 — append-only ledger compensation.
// A compensating journal references a prior posted journal and carries
// sign-reversed entries. The ledger never mutates or deletes existing
// entries; the compensating journal is a fresh, balanced posting whose
// net effect cancels the original. Aggregate-side invariant enforcement
// (JournalKind/CompensationReference coupling, balance check) lands in
// pipeline batch B3 — this command provides the control-plane contract.
public sealed record CompensationReference(
    Guid OriginalJournalId,
    string Reason);

public sealed record PostCompensatingJournalCommand(
    Guid LedgerId,
    Guid JournalId,
    IReadOnlyList<JournalEntryInput> Entries,
    CompensationReference Compensation) : IHasAggregateId
{
    public Guid AggregateId => LedgerId;
}
