using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CanAllocateSpecification : Specification<CapitalAccountAggregate>
{
    private readonly Amount _amount;

    public CanAllocateSpecification(Amount amount)
    {
        _amount = amount;
    }

    public override bool IsSatisfiedBy(CapitalAccountAggregate entity) =>
        entity.Status == CapitalAccountStatus.Active &&
        _amount.Value > 0 &&
        _amount.Value <= entity.AvailableBalance.Value;
}
