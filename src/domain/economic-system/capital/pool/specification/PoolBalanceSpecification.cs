using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public sealed class PoolBalanceSpecification : Specification<CapitalPoolAggregate>
{
    private readonly Amount _expectedAccountSum;

    public PoolBalanceSpecification(Amount expectedAccountSum)
    {
        _expectedAccountSum = expectedAccountSum;
    }

    public override bool IsSatisfiedBy(CapitalPoolAggregate entity)
    {
        return entity.TotalCapital.Value <= _expectedAccountSum.Value;
    }
}
