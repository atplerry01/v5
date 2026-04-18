using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed record JournalEntryAddedEvent(
    JournalId JournalId,
    Guid EntryId,
    Guid AccountId,
    Amount Amount,
    Currency Currency,
    BookingDirection Direction,
    // Phase 6 T6.1 — optional FX rate snapshot. Null on single-currency
    // ledger entries; carries the RateId + Rate value that was in force at
    // FxLockStep when a cross-currency transaction posts. Replay-safe: the
    // event body itself is the authoritative record — no re-resolution.
    Guid? FxRateId = null,
    decimal? FxRate = null) : DomainEvent;
