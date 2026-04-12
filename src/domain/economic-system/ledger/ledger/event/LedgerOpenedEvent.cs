using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record LedgerOpenedEvent(
    LedgerId LedgerId,
    Currency Currency,
    Timestamp OpenedAt) : DomainEvent;
