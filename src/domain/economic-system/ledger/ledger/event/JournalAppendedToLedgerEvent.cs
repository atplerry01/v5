using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record JournalAppendedToLedgerEvent(
    LedgerId LedgerId,
    Guid JournalId,
    Timestamp AppendedAt) : DomainEvent;
