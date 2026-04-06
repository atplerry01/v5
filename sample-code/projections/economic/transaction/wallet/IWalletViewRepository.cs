using Whycespace.Projections.Economic;

namespace Whycespace.Projections.Economic.Transaction.Wallet;

public interface IWalletViewRepository
{
    Task SaveAsync(WalletBalanceReadModel model, CancellationToken ct = default);
    Task<WalletBalanceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
