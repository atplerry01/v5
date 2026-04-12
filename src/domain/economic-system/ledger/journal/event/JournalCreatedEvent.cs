using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed record JournalCreatedEvent(
    JournalId JournalId,
    Timestamp CreatedAt) : DomainEvent;
