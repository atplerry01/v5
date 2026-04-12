using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed record JournalPostedEvent(
    JournalId JournalId,
    Amount TotalDebit,
    Amount TotalCredit,
    Timestamp PostedAt) : DomainEvent;
