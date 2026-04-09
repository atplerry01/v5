using System.Diagnostics;
using Whyce.Platform.Host.Adapters;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): in-process
/// concrete <see cref="IPostgresPoolSnapshotProvider"/>. Combines
/// the canonical PC-4 in-process pool state (read from
/// <see cref="PostgresPoolMetrics.EnumerateState"/>) with the
/// declared <see cref="PostgresPoolCatalog"/> envelope to produce a
/// per-pool snapshot. Pure read — no SQL, no DB connections, no
/// MeterListener.
/// </summary>
public sealed class PostgresPoolSnapshotProvider : IPostgresPoolSnapshotProvider
{
    // HC-6 PATCH: failure detection window. Cumulative failure counts
    // are no longer used by the evaluator; instead, the provider
    // reports the number of acquisition failures observed in this
    // sliding window so a transient outage cannot permanently latch
    // the pool into NotReady. 60s is the canonical default within
    // the documented HC-6 30–60s envelope.
    private static readonly TimeSpan FailureWindowSetting = TimeSpan.FromSeconds(60);

    private readonly PostgresPoolCatalog _catalog;
    private readonly IClock _clock;

    public PostgresPoolSnapshotProvider(PostgresPoolCatalog catalog, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(clock);
        _catalog = catalog;
        _clock = clock;
    }

    public IReadOnlyList<PostgresPoolSnapshot> GetSnapshot()
    {
        // HC-6 PATCH: capture both the wall-clock now (for the
        // snapshot timestamp) and the Stopwatch tick now (for the
        // failure-window cutoff) exactly once, then apply both
        // uniformly to every per-pool snapshot. This guarantees
        // every snapshot in a single probe shares the same
        // TimestampUtc and the same window boundary.
        var nowUtc = _clock.UtcNow.UtcDateTime;
        var nowStopwatchTicks = Stopwatch.GetTimestamp();
        var failureWindowStopwatchTicks =
            (long)(FailureWindowSetting.TotalSeconds * Stopwatch.Frequency);
        var failureCutoffTicks = nowStopwatchTicks - failureWindowStopwatchTicks;

        var byName = new Dictionary<string, PostgresPoolMetrics.PoolStateView>(StringComparer.Ordinal);
        foreach (var (poolName, view) in PostgresPoolMetrics.EnumerateState())
        {
            byName[poolName] = view;
        }

        // Iterate the declared catalog as the authoritative pool set,
        // so a pool that has not yet been touched still appears in the
        // snapshot (with zero counters). This avoids a startup window
        // in which the health check sees zero pools and falsely
        // reports HEALTHY before any acquisition has run.
        var result = new List<PostgresPoolSnapshot>(_catalog.MaxPoolSizeByName.Count);
        foreach (var (poolName, maxConnections) in _catalog.MaxPoolSizeByName)
        {
            byName.TryGetValue(poolName, out var view);
            var avgWaitMs = view.WaitObservationCount > 0
                ? (view.TotalWaitMicros / 1000d) / view.WaitObservationCount
                : 0d;
            // HC-6: AcquisitionFailures must fit int per the contract.
            // Cumulative long is clamped at int.MaxValue — beyond that
            // the rule (> 0) has long since fired anyway.
            var failures = view.TotalFailures > int.MaxValue
                ? int.MaxValue
                : (int)view.TotalFailures;
            // HC-6 PATCH: windowed RecentFailures via the bucket's
            // own queue, evaluated against the single per-probe
            // cutoff captured above.
            var recentFailures = PostgresPoolMetrics.CountRecentFailures(poolName, failureCutoffTicks);
            result.Add(new PostgresPoolSnapshot(
                PoolName: poolName,
                MaxConnections: maxConnections,
                InUseConnections: view.CurrentInFlight,
                PendingAcquisitions: 0, // not knowable from Npgsql public API; reserved.
                AcquisitionFailures: failures,
                AvgWaitMs: avgWaitMs,
                TimestampUtc: nowUtc,
                RecentFailures: recentFailures,
                FailureWindow: FailureWindowSetting));
        }
        return result;
    }
}
