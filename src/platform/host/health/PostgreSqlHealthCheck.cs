using System.Diagnostics;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): capacity-aware
/// readiness check. Pre-HC-6 this issued a <c>SELECT 1</c>
/// connectivity probe per /Health invocation. HC-6 replaces the
/// connectivity probe with a pure read of the in-process
/// <see cref="IPostgresPoolSnapshotProvider"/> followed by the
/// canonical <see cref="PostgresPoolHealthEvaluator"/> rule. The
/// check no longer opens database connections and no longer issues
/// SQL.
///
/// Connectivity is now an indirect signal: a true outage causes
/// every adapter acquisition to fail, the cumulative
/// <c>AcquisitionFailures</c> counter ticks, and the evaluator
/// fails the check on the very next probe under
/// <c>ReasonAcquisitionFailures</c>.
/// </summary>
public sealed class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly IPostgresPoolSnapshotProvider _snapshotProvider;

    public string Name => "postgres";

    public PostgreSqlHealthCheck(IPostgresPoolSnapshotProvider snapshotProvider)
    {
        ArgumentNullException.ThrowIfNull(snapshotProvider);
        _snapshotProvider = snapshotProvider;
    }

    public Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        var snapshots = _snapshotProvider.GetSnapshot();
        var result = PostgresPoolHealthEvaluator.Evaluate(snapshots);
        sw.Stop();

        return result.State switch
        {
            PostgresPoolHealthState.Healthy =>
                Task.FromResult(new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds)),
            PostgresPoolHealthState.Degraded =>
                Task.FromResult(new HealthCheckResult(
                    Name, true, "DEGRADED", sw.ElapsedMilliseconds,
                    string.Join(",", result.Reasons))),
            _ =>
                Task.FromResult(new HealthCheckResult(
                    Name, false, "DOWN", sw.ElapsedMilliseconds,
                    string.Join(",", result.Reasons))),
        };
    }
}
