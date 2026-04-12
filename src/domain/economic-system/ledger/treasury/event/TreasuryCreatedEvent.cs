using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed record TreasuryCreatedEvent(
    TreasuryId TreasuryId,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
