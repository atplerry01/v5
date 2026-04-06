using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Manages wallet balance snapshots for rebuild acceleration.
///
/// Every N events (configurable, default 1000):
///   → persist snapshot to IProjectionStore
///
/// On rebuild:
///   → load snapshot
///   → replay only events after snapshot.LastEventTimestamp
///
/// Uses IProjectionStore — NO direct DB usage.
/// </summary>
public sealed class EconomicSnapshotService
{
    private const string SnapshotProjection = "economic.wallet-balance.snapshot";
    private const string SnapshotCounterProjection = "economic.wallet-balance.snapshot-counter";

    private readonly IProjectionStore _store;
    private readonly EconomicReadStore _readStore;
    private readonly IClock _clock;
    private readonly int _snapshotInterval;

    public EconomicSnapshotService(
        IProjectionStore store,
        EconomicReadStore readStore,
        IClock clock,
        int snapshotInterval = 1000)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _snapshotInterval = snapshotInterval;
    }

    /// <summary>
    /// Called after each ledger event is applied.
    /// Persists snapshot every N events per wallet.
    /// </summary>
    public async Task MaybeSnapshotAsync(
        string walletId,
        string currencyCode,
        CancellationToken ct = default)
    {
        var compositeKey = WalletBalanceReadModel.CompositeKey(walletId, currencyCode);
        var counterKey = $"counter:{compositeKey}";

        // Increment counter
        var counter = await _store.GetAsync<SnapshotCounter>(SnapshotCounterProjection, counterKey, ct);
        var newCount = (counter?.Count ?? 0) + 1;

        await _store.SetAsync(SnapshotCounterProjection, counterKey,
            new SnapshotCounter { Count = newCount }, ct);

        if (newCount % _snapshotInterval != 0)
            return;

        // Take snapshot
        var balance = await _readStore.GetWalletBalanceAsync(compositeKey, ct);
        if (balance is null) return;

        var snapshot = new WalletBalanceSnapshot
        {
            WalletId = balance.WalletId,
            CurrencyCode = balance.CurrencyCode,
            Balance = balance.Balance,
            TotalCredits = balance.TotalCredits,
            TotalDebits = balance.TotalDebits,
            EntryCount = balance.EntryCount,
            LastEventTimestamp = balance.LastEventTimestamp,
            LastEventVersion = balance.LastEventVersion,
            SnapshotTakenAt = _clock.UtcNowOffset
        };

        await _store.SetAsync(SnapshotProjection, compositeKey, snapshot, ct);
    }

    /// <summary>
    /// Loads a snapshot for a specific wallet + currency.
    /// Returns null if no snapshot exists.
    /// </summary>
    public Task<WalletBalanceSnapshot?> LoadSnapshotAsync(
        string walletId,
        CancellationToken ct = default)
    {
        // Load any snapshot for this wallet (across currencies)
        return _store.GetAsync<WalletBalanceSnapshot>(SnapshotProjection, walletId, ct);
    }

    /// <summary>
    /// Loads a snapshot for a specific wallet + currency composite key.
    /// </summary>
    public Task<WalletBalanceSnapshot?> LoadSnapshotAsync(
        string walletId,
        string currencyCode,
        CancellationToken ct = default)
    {
        var key = WalletBalanceReadModel.CompositeKey(walletId, currencyCode);
        return _store.GetAsync<WalletBalanceSnapshot>(SnapshotProjection, key, ct);
    }

    /// <summary>
    /// Loads all wallet balance snapshots.
    /// Used by rebuilder to find the earliest snapshot timestamp.
    /// </summary>
    public Task<IReadOnlyList<WalletBalanceSnapshot>> LoadAllSnapshotsAsync(
        CancellationToken ct = default)
    {
        return _store.GetAllAsync<WalletBalanceSnapshot>(SnapshotProjection, ct);
    }
}

internal sealed class SnapshotCounter
{
    public int Count { get; init; }
}
