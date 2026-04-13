using Npgsql;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed idempotency store. Tracks processed command keys
/// in the idempotency_keys table for duplicate detection.
///
/// phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): all four hot-path
/// methods now thread the request/host-shutdown CancellationToken into
/// the underlying ExecuteNonQueryAsync / ExecuteScalarAsync calls. The
/// empty-paren forms are gone. SQL and pool-acquisition logic are
/// unchanged.
/// </summary>
public sealed class PostgresIdempotencyStoreAdapter : IIdempotencyStore
{
    private readonly EventStoreDataSource _dataSource;

    // phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): connection lifecycle
    // moved to the declared event-store pool. Query logic unchanged.
    public PostgresIdempotencyStoreAdapter(EventStoreDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
    }

    /// <summary>
    /// phase1.5-S5.2.2 / KC-2 (IDEMPOTENCY-COALESCE-01): single
    /// round-trip claim. The <c>INSERT ... ON CONFLICT DO NOTHING</c>
    /// either inserts the row (returns affected=1, this caller is
    /// first-seen) or no-ops on the existing row (returns affected=0,
    /// this caller is a duplicate). One pool acquisition per
    /// invocation, replacing the pre-KC-2 ExistsAsync + MarkAsync
    /// two-acquisition shape on the hot path.
    /// </summary>
    public async Task<bool> TryClaimAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName);

        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO idempotency_keys (key, created_at)
            VALUES (@key, NOW())
            ON CONFLICT (key) DO NOTHING
            """,
            conn);
        cmd.Parameters.AddWithValue("key", key);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected == 1;
    }

    /// <summary>
    /// phase1.5-S5.2.2 / KC-2: rollback for a claim acquired by
    /// <see cref="TryClaimAsync"/> when the inner pipeline failed.
    /// Idempotent — DELETE on a non-existent key is a no-op. Only
    /// reached on the failure path.
    /// </summary>
    public async Task ReleaseAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName);

        await using var cmd = new NpgsqlCommand(
            "DELETE FROM idempotency_keys WHERE key = @key",
            conn);
        cmd.Parameters.AddWithValue("key", key);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    [System.Obsolete("phase1.5-S5.2.2 / KC-2: use TryClaimAsync instead.")]
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName);

        await using var cmd = new NpgsqlCommand(
            "SELECT 1 FROM idempotency_keys WHERE key = @key LIMIT 1",
            conn);
        cmd.Parameters.AddWithValue("key", key);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    [System.Obsolete("phase1.5-S5.2.2 / KC-2: use TryClaimAsync instead.")]
    public async Task MarkAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName);

        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO idempotency_keys (key, created_at)
            VALUES (@key, NOW())
            ON CONFLICT (key) DO NOTHING
            """,
            conn);
        cmd.Parameters.AddWithValue("key", key);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
