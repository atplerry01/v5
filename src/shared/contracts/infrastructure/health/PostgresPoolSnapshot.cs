namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): immutable
/// per-pool capacity snapshot read by <c>PostgreSqlHealthCheck</c>
/// and <c>RuntimeStateAggregator</c>. Sourced from the canonical
/// in-process pool tracking on the existing PC-4
/// <c>PostgresPoolMetrics.OpenInstrumentedAsync</c> seam — no new
/// instrumentation site, no <c>MeterListener</c>, no counter
/// scraping.
///
/// <c>PendingAcquisitions</c> is not currently knowable: Npgsql does
/// not expose its internal pool wait queue via any public Meter /
/// EventCounter / DiagnosticListener path. The field is reserved
/// (always 0 in HC-6) so the contract does not need to widen when a
/// future Npgsql version exposes it.
/// </summary>
public sealed record PostgresPoolSnapshot(
    string PoolName,
    int MaxConnections,
    int InUseConnections,
    int PendingAcquisitions,
    int AcquisitionFailures,
    double AvgWaitMs,
    DateTime TimestampUtc,
    int RecentFailures,
    TimeSpan FailureWindow);
