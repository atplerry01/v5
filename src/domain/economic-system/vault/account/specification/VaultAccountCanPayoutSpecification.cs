using Whycespace.Domain.EconomicSystem.Vault.Slice;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

/// <summary>
/// Reusable payout-debit eligibility rule: the vault must be Active, the
/// targeted slice must be Slice1 (the doctrine-locked payout slice), the
/// amount must be strictly positive, and the amount must not exceed the
/// vault's free liquidity on Slice1.
/// </summary>
public sealed class VaultAccountCanPayoutSpecification : Specification<VaultAccountAggregate>
{
    private readonly SliceType _slice;
    private readonly Amount _amount;

    public VaultAccountCanPayoutSpecification(SliceType slice, Amount amount)
    {
        _slice = slice;
        _amount = amount;
    }

    public override bool IsSatisfiedBy(VaultAccountAggregate account)
    {
        if (account is null) return false;
        if (account.Status != VaultAccountStatus.Active) return false;
        if (_slice != SliceType.Slice1) return false;
        if (_amount.Value <= 0m) return false;
        if (_amount.Value > account.Metrics.Free.Value) return false;
        return true;
    }
}
