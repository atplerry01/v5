using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public sealed class PoolAggregationService
{
    public bool ReconcilePool(CapitalPoolAggregate pool, IReadOnlyList<Amount> accountBalances)
    {
        var sum = accountBalances.Aggregate(0m, (acc, balance) => acc + balance.Value);
        return pool.TotalCapital.Value == sum;
    }
}
