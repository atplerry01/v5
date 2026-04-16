using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

/// <summary>
/// Reusable funding eligibility rule: the vault must be Active, the inbound
/// currency must match the vault's declared currency, and the amount must be
/// strictly positive. Funding always lands on Slice1 (liquidity gateway) —
/// the Slice1 routing constraint is a structural invariant of
/// VaultAccountAggregate and is not re-checked here.
/// </summary>
public sealed class VaultAccountCanFundSpecification : Specification<VaultAccountAggregate>
{
    private readonly Amount _amount;
    private readonly Currency _currency;

    public VaultAccountCanFundSpecification(Amount amount, Currency currency)
    {
        _amount = amount;
        _currency = currency;
    }

    public override bool IsSatisfiedBy(VaultAccountAggregate account)
    {
        if (account is null) return false;
        if (account.Status != VaultAccountStatus.Active) return false;
        if (_amount.Value <= 0m) return false;
        if (account.Currency != _currency) return false;
        return true;
    }
}
