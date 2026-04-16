using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

/// <summary>
/// Reusable investment eligibility rule: the vault must be Active, the
/// requested currency must match the vault's declared currency, the amount
/// must be strictly positive, and the amount must not exceed the vault's
/// free liquidity on Slice1. Investment always flows Slice1 → Slice2 — the
/// Slice routing is a structural invariant of VaultAccountAggregate.
/// </summary>
public sealed class VaultAccountCanInvestSpecification : Specification<VaultAccountAggregate>
{
    private readonly Amount _amount;
    private readonly Currency _currency;

    public VaultAccountCanInvestSpecification(Amount amount, Currency currency)
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
        if (_amount.Value > account.Metrics.Free.Value) return false;
        return true;
    }
}
