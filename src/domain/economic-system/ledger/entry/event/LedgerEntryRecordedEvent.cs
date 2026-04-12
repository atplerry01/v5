using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Entry;

public sealed record LedgerEntryRecordedEvent(
    EntryId EntryId,
    Guid JournalId,
    Guid AccountId,
    Amount Amount,
    Currency Currency,
    EntryDirection Direction,
    Timestamp CreatedAt) : DomainEvent;
