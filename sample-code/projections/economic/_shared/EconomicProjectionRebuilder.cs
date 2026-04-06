using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Rebuilds economic projections from the event store.
/// Event store = source of truth. Projections are derived state.
///
/// Supports:
///   - Full rebuild (all projections from zero)
///   - Per-entity rebuild (WalletId, IdentityId, TransactionId)
///   - Optional clear before rebuild
///   - Snapshot-aware: loads snapshot, replays only newer events
///
/// Replay-safe: handlers are idempotent, rebuild produces identical state.
/// </summary>
public interface IEconomicProjectionRebuilder
{
    Task RebuildAllAsync(bool clearBeforeRebuild = true, CancellationToken ct = default);
    Task RebuildWalletAsync(string walletId, CancellationToken ct = default);
    Task RebuildIdentityAsync(string identityId, CancellationToken ct = default);
    Task RebuildTransactionAsync(string transactionId, CancellationToken ct = default);
}

public sealed class EconomicProjectionRebuilder : IEconomicProjectionRebuilder
{
    private readonly IProjectionEventReader _eventReader;
    private readonly IProjectionStore _projectionStore;
    private readonly EconomicReadStore _readStore;
    private readonly IClock _clock;
    private readonly EconomicSnapshotService? _snapshotService;

    private static readonly string[] AllEconomicEventTypes =
    [
        "economic.ledger.entry-recorded",
        "economic.transaction.initiated",
        "economic.transaction.approved",
        "economic.transaction.rejected",
        "economic.transaction.completed",
        "economic.transaction.settled",
        "economic.enforcement.applied",
        "economic.enforcement.released",
        "economic.limit.exceeded"
    ];

    private static readonly string[] WalletEventTypes =
    [
        "economic.ledger.entry-recorded"
    ];

    private static readonly string[] TransactionEventTypes =
    [
        "economic.transaction.initiated",
        "economic.transaction.approved",
        "economic.transaction.rejected",
        "economic.transaction.completed",
        "economic.transaction.settled"
    ];

    private static readonly string[] IdentityEventTypes =
    [
        "economic.enforcement.applied",
        "economic.enforcement.released",
        "economic.transaction.initiated",
        "economic.transaction.approved",
        "economic.limit.exceeded"
    ];

    public EconomicProjectionRebuilder(
        IProjectionEventReader eventReader,
        IProjectionStore projectionStore,
        EconomicReadStore readStore,
        IClock clock,
        EconomicSnapshotService? snapshotService = null)
    {
        _eventReader = eventReader ?? throw new ArgumentNullException(nameof(eventReader));
        _projectionStore = projectionStore ?? throw new ArgumentNullException(nameof(projectionStore));
        _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _snapshotService = snapshotService;
    }

    public async Task RebuildAllAsync(bool clearBeforeRebuild = true, CancellationToken ct = default)
    {
        DateTimeOffset? replayAfter = null;

        // Load snapshot if available
        if (_snapshotService is not null && !clearBeforeRebuild)
        {
            var snapshots = await _snapshotService.LoadAllSnapshotsAsync(ct);
            if (snapshots.Count > 0)
            {
                replayAfter = snapshots.Min(s => s.LastEventTimestamp);
            }
        }

        if (clearBeforeRebuild)
        {
            // Reset all projection checkpoints
            await _projectionStore.SetCheckpointAsync("economic.wallet-balance", 0, ct);
            await _projectionStore.SetCheckpointAsync("economic.transaction-history", 0, ct);
            await _projectionStore.SetCheckpointAsync("economic.enforcement", 0, ct);
            await _projectionStore.SetCheckpointAsync("economic.limit-usage", 0, ct);
        }

        // Build registration list with all handlers
        var registrations = EconomicProjectionRegistration.BuildRegistrations(_readStore, _clock);

        // Replay all events (ordered by timestamp from event store)
        var events = await _eventReader.ReadAllAsync(after: replayAfter, cancellationToken: ct);

        foreach (var @event in events.OrderBy(e => e.Timestamp).ThenBy(e => e.Version))
        {
            if (!AllEconomicEventTypes.Contains(@event.EventType))
                continue;

            foreach (var registration in registrations)
            {
                if (registration.EventTypes.Contains(@event.EventType))
                {
                    await registration.Handler(@event, _projectionStore, ct);
                }
            }
        }
    }

    public async Task RebuildWalletAsync(string walletId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(walletId);

        DateTimeOffset? replayAfter = null;

        // Load snapshot if available
        if (_snapshotService is not null)
        {
            var snapshot = await _snapshotService.LoadSnapshotAsync(walletId, ct);
            if (snapshot is not null)
            {
                replayAfter = snapshot.LastEventTimestamp;
            }
        }

        var handler = new WalletBalanceProjectionHandler(_readStore, _clock);
        var events = await _eventReader.ReadAllAsync(after: replayAfter, cancellationToken: ct);

        foreach (var @event in events.OrderBy(e => e.Timestamp).ThenBy(e => e.Version))
        {
            if (!WalletEventTypes.Contains(@event.EventType))
                continue;

            await handler.HandleAsync(@event, _projectionStore, ct);
        }
    }

    public async Task RebuildIdentityAsync(string identityId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(identityId);

        var enforcementHandler = new EnforcementProjectionHandler(_readStore, _clock);
        var limitHandler = new LimitUsageProjectionHandler(_readStore, _clock);

        var events = await _eventReader.ReadAllAsync(cancellationToken: ct);

        foreach (var @event in events.OrderBy(e => e.Timestamp).ThenBy(e => e.Version))
        {
            if (!IdentityEventTypes.Contains(@event.EventType))
                continue;

            if (enforcementHandler.EventTypes.Contains(@event.EventType))
                await enforcementHandler.HandleAsync(@event, _projectionStore, ct);

            if (limitHandler.EventTypes.Contains(@event.EventType))
                await limitHandler.HandleAsync(@event, _projectionStore, ct);
        }
    }

    public async Task RebuildTransactionAsync(string transactionId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(transactionId);

        var handler = new TransactionHistoryProjectionHandler(_readStore, _clock);
        var events = await _eventReader.ReadAllAsync(cancellationToken: ct);

        foreach (var @event in events.OrderBy(e => e.Timestamp).ThenBy(e => e.Version))
        {
            if (!TransactionEventTypes.Contains(@event.EventType))
                continue;

            await handler.HandleAsync(@event, _projectionStore, ct);
        }
    }
}
