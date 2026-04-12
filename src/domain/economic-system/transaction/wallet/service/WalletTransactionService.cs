namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed class WalletTransactionService
{
    public Guid GetAccountId(WalletAggregate wallet)
    {
        if (wallet.AccountId == Guid.Empty)
            throw WalletErrors.NoAccountMapped();

        return wallet.AccountId;
    }
}
