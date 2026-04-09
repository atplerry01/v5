using System.Diagnostics.Metrics;
using Npgsql;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): canonical observability
/// surface for the declared Npgsql logical pools. A dedicated
/// <c>Whyce.Postgres</c> meter exports two counters tagged by the
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
    public static readonly Meter Meter = new("Whyce.Postgres", "1.0");

    private static readonly Counter<long> Acquisitions =
        Meter.CreateCounter<long>("postgres.pool.acquisitions");

    private static readonly Counter<long> AcquisitionFailures =
        Meter.CreateCounter<long>("postgres.pool.acquisition_failures");

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
        try
        {
            var conn = await dataSource.OpenConnectionAsync(ct);
            Acquisitions.Add(1, new KeyValuePair<string, object?>("pool", poolName));
            return conn;
        }
        catch (Exception ex)
        {
            AcquisitionFailures.Add(1,
                new KeyValuePair<string, object?>("pool", poolName),
                new KeyValuePair<string, object?>("reason", ex.GetType().Name));
            throw;
        }
    }
}
