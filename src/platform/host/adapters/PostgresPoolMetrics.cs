using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Npgsql;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): canonical observability
/// surface for the declared Npgsql logical pools. A dedicated
/// <c>Whycespace.Postgres</c> meter exports two counters tagged by the
/// logical pool name:
///
///   - <c>postgres.pool.acquisitions</c>          — every successful
///                                                  connection acquisition.
///   - <c>postgres.pool.acquisition_failures</c>  — every failure
///                                                  (timeout, transport,
///                                                  exhaustion), tagged
///                                                  with the exception
///                                                  type as <c>reason</c>.
///
/// Adapters acquire connections via <see cref="OpenInstrumentedAsync"/>
/// rather than <c>NpgsqlDataSource.OpenConnectionAsync</c> directly so
/// every acquisition flows through this seam. Native Npgsql
/// <c>EventCounters</c> are not yet bridged to this meter (the .NET 10
/// Npgsql package exposes them via <c>System.Diagnostics.DiagnosticListener</c>
/// rather than <c>Meter</c>); the explicit acquisition counters here
/// are sufficient for §5.3.x load work and avoid coupling to the
/// internal counter shape.
/// </summary>
public static class PostgresPoolMetrics
{
    public static readonly Meter Meter = new("Whycespace.Postgres", "1.0");

    private static readonly Counter<long> Acquisitions =
        Meter.CreateCounter<long>("postgres.pool.acquisitions");

    private static readonly Counter<long> AcquisitionFailures =
        Meter.CreateCounter<long>("postgres.pool.acquisition_failures");

    // phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): in-process
    // state tracking on the same acquisition path that already
    // emits the Meter counters above. Lives here so HC-6 can read
    // pool capacity / failure / wait state without a MeterListener,
    // Prometheus parser, or counter scraper — all of which are
    // explicitly forbidden by the §5.2.4 discipline. The Meter
    // emissions remain unchanged; this is a parallel write into
    // process-local Interlocked counters that the new HC-6
    // PostgresPoolSnapshotProvider reads at health-probe time.
    private static readonly ConcurrentDictionary<string, PoolStateBucket> _state =
        new(StringComparer.Ordinal);

    /// <summary>
    /// HC-6: read-only access to per-pool in-process state. Returns
    /// a stable enumeration of (pool name, bucket snapshot) pairs.
    /// Buckets are exposed as primitive struct copies so callers
    /// observe a coherent point-in-time view without holding any
    /// reference to the live mutable bucket.
    /// </summary>
    public static IEnumerable<(string PoolName, PoolStateView View)> EnumerateState()
    {
        foreach (var kvp in _state)
        {
            yield return (kvp.Key, kvp.Value.Read());
        }
    }

    public readonly record struct PoolStateView(
        long TotalAcquisitions,
        long TotalFailures,
        int CurrentInFlight,
        long TotalWaitMicros,
        long WaitObservationCount);

    /// <summary>
    /// HC-6 PATCH (windowed failure detection): returns the count of
    /// acquisition failures recorded against <paramref name="poolName"/>
    /// whose <see cref="Stopwatch.GetTimestamp"/> is at or after
    /// <paramref name="cutoffTicks"/>. Older entries are evicted as a
    /// side effect so the queue cannot grow without bound. The cutoff
    /// is supplied by the caller (the snapshot provider) so the
    /// window boundary is computed once per probe and applied
    /// uniformly across all pools.
    /// </summary>
    public static int CountRecentFailures(string poolName, long cutoffTicks)
    {
        if (!_state.TryGetValue(poolName, out var bucket))
            return 0;
        return bucket.CountRecentFailures(cutoffTicks);
    }

    private sealed class PoolStateBucket
    {
        public long TotalAcquisitions;
        public long TotalFailures;
        public int CurrentInFlight;
        public long TotalWaitMicros;
        public long WaitObservationCount;

        // HC-6 PATCH: monotonic Stopwatch ticks for each acquisition
        // failure. Read/trimmed under _failuresLock by
        // CountRecentFailures so the windowed view stays bounded.
        // Stopwatch is monotonic and is not subject to wall-clock
        // skew or to the $9 deterministic-time rule (which targets
        // domain logic, not health-probe instrumentation).
        private readonly object _failuresLock = new();
        private readonly Queue<long> _recentFailureTicks = new();

        public void RecordFailure(long nowTicks)
        {
            lock (_failuresLock)
            {
                _recentFailureTicks.Enqueue(nowTicks);
            }
        }

        public int CountRecentFailures(long cutoffTicks)
        {
            lock (_failuresLock)
            {
                while (_recentFailureTicks.Count > 0 && _recentFailureTicks.Peek() < cutoffTicks)
                {
                    _recentFailureTicks.Dequeue();
                }
                return _recentFailureTicks.Count;
            }
        }

        public PoolStateView Read() => new(
            Volatile.Read(ref TotalAcquisitions),
            Volatile.Read(ref TotalFailures),
            Volatile.Read(ref CurrentInFlight),
            Volatile.Read(ref TotalWaitMicros),
            Volatile.Read(ref WaitObservationCount));
    }

    /// <summary>
    /// Acquires a connection from the supplied <see cref="NpgsqlDataSource"/>
    /// and increments <c>postgres.pool.acquisitions</c> on success or
    /// <c>postgres.pool.acquisition_failures</c> on any throw. The
    /// <paramref name="poolName"/> tag identifies the logical pool
    /// (e.g. <c>"event-store"</c>, <c>"chain"</c>).
    ///
    /// Acquisition failures are re-thrown unchanged — this seam never
    /// swallows or remaps the underlying exception. Callers see the
    /// same Npgsql exception types they would see calling
    /// <c>OpenConnectionAsync</c> directly.
    /// </summary>
    public static async Task<NpgsqlConnection> OpenInstrumentedAsync(
        this NpgsqlDataSource dataSource,
        string poolName,
        CancellationToken ct = default)
    {
        var bucket = _state.GetOrAdd(poolName, _ => new PoolStateBucket());
        var startTicks = Stopwatch.GetTimestamp();
        try
        {
            var conn = await dataSource.OpenConnectionAsync(ct);
            var elapsedMicros = (long)(Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds * 1000d);
            Interlocked.Increment(ref bucket.TotalAcquisitions);
            Interlocked.Increment(ref bucket.CurrentInFlight);
            Interlocked.Add(ref bucket.TotalWaitMicros, elapsedMicros);
            Interlocked.Increment(ref bucket.WaitObservationCount);
            // HC-6: decrement in-flight when the caller disposes the
            // connection (returning it to the pool). The closure
            // captures `bucket` by reference so the decrement targets
            // the correct pool even under concurrent acquisitions
            // across pools.
            //
            // phase1.5-S5.2.5 / TB-1: the original implementation
            // subscribed to Component.Disposed via NpgsqlConnection's
            // base class. The current Npgsql version no longer
            // supports the Disposed event and throws
            // NotSupportedException at subscription time
            // ("The Disposed event isn't supported by Npgsql. Use
            // DbConnection.StateChange instead."). This was a latent
            // S0 production bug — every code path through
            // OpenInstrumentedAsync (outbox publisher, outbox enqueue,
            // event store, idempotency store, depth sampler) was
            // broken at runtime against the upgraded Npgsql, hidden
            // because the only Postgres-backed integration tests that
            // exercised this seam were broken at compile time and
            // therefore never ran. TB-1 surfaced both at once. The
            // canonical replacement is the StateChange event filtered
            // to the Closed transition — NpgsqlConnection enters
            // Closed during Dispose for any previously-Open
            // connection, which is exactly the moment HC-6 needs to
            // observe the return-to-pool.
            conn.StateChange += (_, args) =>
            {
                if (args.CurrentState == System.Data.ConnectionState.Closed)
                    Interlocked.Decrement(ref bucket.CurrentInFlight);
            };
            Acquisitions.Add(1, new KeyValuePair<string, object?>("pool", poolName));
            return conn;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref bucket.TotalFailures);
            // HC-6 PATCH: record the failure into the windowed queue
            // so the snapshot provider can compute RecentFailures
            // without latching on the cumulative TotalFailures.
            bucket.RecordFailure(Stopwatch.GetTimestamp());
            AcquisitionFailures.Add(1,
                new KeyValuePair<string, object?>("pool", poolName),
                new KeyValuePair<string, object?>("reason", ex.GetType().Name));
            throw;
        }
    }
}
