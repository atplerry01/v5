using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed class CanInitiateTransactionSpecification : Specification<WalletAggregate>
{
    public override bool IsSatisfiedBy(WalletAggregate wallet) =>
        wallet.Status == WalletStatus.Active &&
        wallet.AccountId != Guid.Empty;
}
