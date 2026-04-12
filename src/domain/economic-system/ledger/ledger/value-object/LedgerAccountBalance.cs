using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record LedgerAccountBalance(
    Guid AccountId,
    Amount DebitTotal,
    Amount CreditTotal,
    Amount NetBalance) : ValueObject;
