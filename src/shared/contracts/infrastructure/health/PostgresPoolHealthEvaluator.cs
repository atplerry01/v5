namespace Whyce.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): pure-function
/// evaluator over a set of <see cref="PostgresPoolSnapshot"/>s.
/// Single source of truth for the pool-level health rule —
/// consumed by both <c>PostgreSqlHealthCheck</c> (for the
/// /Health diagnostics view) and <c>RuntimeStateAggregator</c>
/// (for the canonical reason emission). Keeping the rule in one
/// pure function eliminates the drift risk that would otherwise
/// arise from evaluating the same thresholds in two places.
///
/// Rule (per pool, hard-coded thresholds from the HC-6 spec):
///   - MaxConnections &lt;= 0                           =&gt; NotReady ("postgres_invalid_pool_config")
///   - InUseConnections &gt;= 0.95 * MaxConnections   =&gt; NotReady ("postgres_pool_exhausted")
///   - RecentFailures &gt; 0 (windowed)                 =&gt; NotReady ("postgres_acquisition_failures")
///   - AvgWaitMs &gt; 100                               =&gt; Degraded ("postgres_high_wait")
///
/// HC-6 PATCH (post-implementation):
///   - Failure detection is windowed via <c>RecentFailures</c> rather
///     than the cumulative <c>AcquisitionFailures</c> counter, so a
///     transient failure no longer permanently latches the pool into
///     NotReady.
///   - An invalid pool configuration (<c>MaxConnections &lt;= 0</c>)
///     short-circuits to NotReady so a misconfigured pool cannot be
///     reported as healthy by accident.
///   - Reason output follows a fixed canonical order regardless of
///     iteration order across pools, so the emitted reason string is
///     deterministic across hosts and probes.
///
/// Aggregation across pools: any NotReady pool dominates;
/// otherwise any Degraded pool yields Degraded; otherwise Healthy.
/// Reasons are deduplicated and emitted in canonical declaration
/// order so output is deterministic.
/// </summary>
public static class PostgresPoolHealthEvaluator
{
    public const string ReasonInvalidPoolConfig = "postgres_invalid_pool_config";
    public const string ReasonPoolExhausted = "postgres_pool_exhausted";
    public const string ReasonAcquisitionFailures = "postgres_acquisition_failures";
    public const string ReasonHighWait = "postgres_high_wait";

    public static PostgresPoolHealthResult Evaluate(IReadOnlyList<PostgresPoolSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        var invalidConfig = false;
        var poolExhausted = false;
        var acquisitionFailures = false;
        var highWait = false;

        foreach (var s in snapshots)
        {
            if (s.MaxConnections <= 0)
            {
                invalidConfig = true;
                continue;
            }
            if (s.InUseConnections >= 0.95d * s.MaxConnections)
                poolExhausted = true;
            if (s.RecentFailures > 0)
                acquisitionFailures = true;
            if (s.AvgWaitMs > 100d)
                highWait = true;
        }

        // HC-6 PATCH: deterministic canonical order — invalid config
        // first (highest-priority NotReady), then the three documented
        // reasons in their fixed order: exhausted, failures, high_wait.
        var reasons = new List<string>(4);
        if (invalidConfig) reasons.Add(ReasonInvalidPoolConfig);
        if (poolExhausted) reasons.Add(ReasonPoolExhausted);
        if (acquisitionFailures) reasons.Add(ReasonAcquisitionFailures);
        if (highWait) reasons.Add(ReasonHighWait);

        var notReady = invalidConfig || poolExhausted || acquisitionFailures;
        var state = notReady
            ? PostgresPoolHealthState.NotReady
            : highWait
                ? PostgresPoolHealthState.Degraded
                : PostgresPoolHealthState.Healthy;

        return new PostgresPoolHealthResult(state, reasons);
    }
}

public enum PostgresPoolHealthState
{
    Healthy = 0,
    Degraded = 1,
    NotReady = 2,
}

public sealed record PostgresPoolHealthResult(
    PostgresPoolHealthState State,
    IReadOnlyList<string> Reasons);
