using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record LedgerUpdatedEvent(
    LedgerId LedgerId,
    Guid JournalId,
    int JournalCount,
    Timestamp UpdatedAt) : DomainEvent;
