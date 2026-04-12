using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed record TreasuryFundAllocatedEvent(
    TreasuryId TreasuryId,
    Amount AllocatedAmount,
    Amount NewBalance) : DomainEvent;
