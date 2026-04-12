using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CanCloseSpecification : Specification<CapitalAccountAggregate>
{
    public override bool IsSatisfiedBy(CapitalAccountAggregate entity) =>
        entity.Status != CapitalAccountStatus.Closed &&
        entity.TotalBalance.Value == 0 &&
        entity.ReservedBalance.Value == 0;
}
