using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

/// <summary>
/// Phase 7 T7.4 — the compensating-journal counterpart of
/// <see cref="JournalCreatedEvent"/>. Emitted once at creation of a
/// Kind=Compensating journal and never again; the
/// <see cref="CompensationReference"/> it carries is the provable
/// ledger-level link from the new balanced journal back to the original
/// it reverses. The ledger remains append-only — no original entry is
/// ever mutated.
/// </summary>
public sealed record JournalCompensationCreatedEvent(
    JournalId JournalId,
    CompensationReference Compensation,
    Timestamp CreatedAt) : DomainEvent;
