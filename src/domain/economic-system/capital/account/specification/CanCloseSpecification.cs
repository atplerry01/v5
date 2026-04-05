using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CanCloseSpecification : Specification<CapitalAccountAggregate>
{
    public override bool IsSatisfiedBy(CapitalAccountAggregate account) =>
        account.Status != CapitalAccountStatus.Closed &&
        account.TotalBalance == 0m &&
        account.ReservedBalance == 0m;
}
