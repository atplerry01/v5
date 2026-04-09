namespace Whyce.Shared.Contracts.Infrastructure.Persistence;

/// <summary>
/// Tunable behavior for a single logical Postgres connection pool.
///
/// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): externalises the previously
/// incidental Npgsql library defaults. The host registers one
/// <c>NpgsqlDataSource</c> per logical pool (currently <c>event-store</c>
/// and <c>chain</c>) sized from a corresponding instance of this record
/// bound at the composition root from configuration. Follows the
/// phase1.6-S1.5 OutboxOptions / phase1.5-S5.2.1 OpaOptions / IntakeOptions
/// precedent — a plain record, no <c>IOptions&lt;T&gt;</c> indirection.
///
/// Defaults are deliberately conservative: an unconfigured deployment is
/// safer (declared, observable, bounded) than the pre-S5.2.1 incidental
/// behavior, while remaining well within typical Postgres
/// <c>max_connections</c> envelopes for a small fleet.
/// </summary>
public sealed record PostgresPoolOptions
{
    /// <summary>
    /// Logical pool identity, used as the <c>pool</c> tag on every
    /// <c>postgres.pool.*</c> metric. Should be a short, stable string
    /// (e.g. <c>"event-store"</c>, <c>"chain"</c>). Required.
    /// </summary>
    public string PoolName { get; init; } = string.Empty;

    /// <summary>
    /// Postgres connection string. The composition root passes the same
    /// connection string the pre-S5.2.1 adapters used; pool sizing and
    /// timeouts are appended to it programmatically via
    /// <c>NpgsqlConnectionStringBuilder</c>. Required.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// Maximum number of physical connections held by this pool. Must
    /// be at least 1. Default 32 — sized so the two declared pools
    /// (event-store + chain) together stay well under the typical
    /// Postgres <c>max_connections=100</c> default for a single host.
    /// </summary>
    public int MaxPoolSize { get; init; } = 32;

    /// <summary>
    /// Minimum number of warm connections kept open by the pool. Must
    /// be 0 or greater. Default 1 (keeps a warm connection so the
    /// first request after an idle window does not pay the full
    /// connect cost).
    /// </summary>
    public int MinPoolSize { get; init; } = 1;

    /// <summary>
    /// Connection acquisition timeout, in seconds. When the pool is
    /// fully busy, callers wait this long before <c>OpenConnectionAsync</c>
    /// throws. Must be at least 1. Default 5 — short enough that
    /// pool exhaustion produces an explicit, observable failure
    /// (counted on <c>postgres.pool.acquisition_failures</c>) instead
    /// of a silent stall.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; init; } = 5;

    /// <summary>
    /// Default per-command timeout, in seconds, applied via the Npgsql
    /// connection string <c>Command Timeout</c>. Must be at least 1.
    /// Default 30 (matches the Npgsql library default — kept explicit
    /// to honor the no-incidental-defaults rule R-10).
    /// </summary>
    public int CommandTimeoutSeconds { get; init; } = 30;
}
