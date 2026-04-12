using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed class TreasuryLiquidityService
{
    public bool HasSufficientLiquidity(TreasuryAggregate treasury, Amount required) =>
        treasury.Balance.Value >= required.Value;
}
