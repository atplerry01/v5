using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed record JournalEntryAddedEvent(
    JournalId JournalId,
    Guid EntryId,
    Guid AccountId,
    Amount Amount,
    Currency Currency,
    BookingDirection Direction) : DomainEvent;
