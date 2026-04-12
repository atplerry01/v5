using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public sealed class CanReduceSpecification : Specification<CapitalPoolAggregate>
{
    private readonly Amount _amount;

    public CanReduceSpecification(Amount amount)
    {
        _amount = amount;
    }

    public override bool IsSatisfiedBy(CapitalPoolAggregate entity)
    {
        return _amount.Value > 0 && _amount.Value <= entity.TotalCapital.Value;
    }
}
