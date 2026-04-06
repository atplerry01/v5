using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Read store abstraction for economic projections.
/// Backed by IProjectionStore (Redis in production, in-memory for tests).
/// </summary>
public sealed class EconomicReadStore
{
    private const string WalletBalanceProjection = "economic.wallet-balance";
    private const string TransactionHistoryProjection = "economic.transaction-history";
    private const string EnforcementProjection = "economic.enforcement";
    private const string LimitUsageProjection = "economic.limit-usage";

    private readonly IProjectionStore _store;

    public EconomicReadStore(IProjectionStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    // ── Wallet Balance (keyed by WalletId:CurrencyCode) ──

    public Task<WalletBalanceReadModel?> GetWalletBalanceAsync(string compositeKey, CancellationToken ct = default)
        => _store.GetAsync<WalletBalanceReadModel>(WalletBalanceProjection, compositeKey, ct);

    public Task<WalletBalanceReadModel?> GetWalletBalanceAsync(string walletId, string currencyCode, CancellationToken ct = default)
        => _store.GetAsync<WalletBalanceReadModel>(WalletBalanceProjection, WalletBalanceReadModel.CompositeKey(walletId, currencyCode), ct);

    public Task SetWalletBalanceAsync(string compositeKey, WalletBalanceReadModel model, CancellationToken ct = default)
        => _store.SetAsync(WalletBalanceProjection, compositeKey, model, ct);

    public Task<IReadOnlyList<WalletBalanceReadModel>> GetAllWalletBalancesAsync(CancellationToken ct = default)
        => _store.GetAllAsync<WalletBalanceReadModel>(WalletBalanceProjection, ct);

    // ── Transaction History ──

    public Task<TransactionHistoryReadModel?> GetTransactionAsync(string transactionId, CancellationToken ct = default)
        => _store.GetAsync<TransactionHistoryReadModel>(TransactionHistoryProjection, transactionId, ct);

    public Task SetTransactionAsync(string transactionId, TransactionHistoryReadModel model, CancellationToken ct = default)
        => _store.SetAsync(TransactionHistoryProjection, transactionId, model, ct);

    public Task<IReadOnlyList<TransactionHistoryReadModel>> GetAllTransactionsAsync(CancellationToken ct = default)
        => _store.GetAllAsync<TransactionHistoryReadModel>(TransactionHistoryProjection, ct);

    // ── Enforcement ──

    public Task<EnforcementReadModel?> GetEnforcementAsync(string identityId, CancellationToken ct = default)
        => _store.GetAsync<EnforcementReadModel>(EnforcementProjection, identityId, ct);

    public Task SetEnforcementAsync(string identityId, EnforcementReadModel model, CancellationToken ct = default)
        => _store.SetAsync(EnforcementProjection, identityId, model, ct);

    public Task RemoveEnforcementAsync(string identityId, CancellationToken ct = default)
        => _store.RemoveAsync(EnforcementProjection, identityId, ct);

    // ── Limit Usage ──

    public Task<LimitUsageReadModel?> GetLimitUsageAsync(string identityId, CancellationToken ct = default)
        => _store.GetAsync<LimitUsageReadModel>(LimitUsageProjection, identityId, ct);

    public Task SetLimitUsageAsync(string identityId, LimitUsageReadModel model, CancellationToken ct = default)
        => _store.SetAsync(LimitUsageProjection, identityId, model, ct);
}
