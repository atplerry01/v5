namespace Whycespace.Projections.Economic;

/// <summary>
/// Query interface for economic read models.
/// Read-only — no mutation, no business logic.
/// </summary>
public sealed class EconomicQuery
{
    private readonly EconomicReadStore _store;

    public EconomicQuery(EconomicReadStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Task<WalletBalanceReadModel?> GetWalletBalanceAsync(string walletId, string currencyCode, CancellationToken ct = default)
        => _store.GetWalletBalanceAsync(walletId, currencyCode, ct);

    public Task<IReadOnlyList<WalletBalanceReadModel>> GetAllWalletBalancesAsync(CancellationToken ct = default)
        => _store.GetAllWalletBalancesAsync(ct);

    public Task<TransactionHistoryReadModel?> GetTransactionAsync(string transactionId, CancellationToken ct = default)
        => _store.GetTransactionAsync(transactionId, ct);

    public Task<IReadOnlyList<TransactionHistoryReadModel>> GetAllTransactionsAsync(CancellationToken ct = default)
        => _store.GetAllTransactionsAsync(ct);

    public Task<EnforcementReadModel?> GetEnforcementAsync(string identityId, CancellationToken ct = default)
        => _store.GetEnforcementAsync(identityId, ct);

    public Task<LimitUsageReadModel?> GetLimitUsageAsync(string identityId, CancellationToken ct = default)
        => _store.GetLimitUsageAsync(identityId, ct);
}
