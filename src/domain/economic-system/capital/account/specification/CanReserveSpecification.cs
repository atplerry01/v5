using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CanReserveSpecification : Specification<CapitalAccountAggregate>
{
    private readonly decimal _amount;

    public CanReserveSpecification(decimal amount)
    {
        _amount = amount;
    }

    public override bool IsSatisfiedBy(CapitalAccountAggregate account) =>
        account.Status == CapitalAccountStatus.Active &&
        _amount > 0m &&
        _amount <= account.AvailableBalance;
}
