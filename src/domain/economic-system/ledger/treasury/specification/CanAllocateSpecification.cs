using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed class CanAllocateSpecification : Specification<TreasuryAggregate>
{
    private readonly Amount _amount;

    public CanAllocateSpecification(Amount amount)
    {
        _amount = amount;
    }

    public override bool IsSatisfiedBy(TreasuryAggregate entity) =>
        _amount.Value > 0 && _amount.Value <= entity.Balance.Value;
}
