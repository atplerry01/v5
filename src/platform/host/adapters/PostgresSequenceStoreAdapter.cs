using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed HSID v2.1 sequence store. Each scope row is updated via
/// an UPSERT that returns the post-increment counter atomically. Mirrors
/// <see cref="PostgresIdempotencyStoreAdapter"/> in connection lifecycle.
///
/// Schema (must exist via migration):
/// <code>
/// CREATE TABLE IF NOT EXISTS hsid_sequences (
///   scope text PRIMARY KEY,
///   next_value bigint NOT NULL
/// );
/// </code>
/// </summary>
public sealed class PostgresSequenceStoreAdapter : ISequenceStore
{
    private readonly string _connectionString;

    public PostgresSequenceStoreAdapter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> NextAsync(string scope)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO hsid_sequences (scope, next_value)
            VALUES (@scope, 1)
            ON CONFLICT (scope) DO UPDATE
              SET next_value = hsid_sequences.next_value + 1
            RETURNING next_value
            """,
            conn);
        cmd.Parameters.AddWithValue("scope", scope);

        var result = await cmd.ExecuteScalarAsync();
        return result is long l ? l : Convert.ToInt64(result);
    }

    public async Task<bool> HealthCheckAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Strict shape check: confirm the table exists AND both required
        // columns are present. Catches partial / drifted migrations as well
        // as missing ones. Per deterministic-id.guard.md G20.
        await using var cmd = new NpgsqlCommand(
            """
            SELECT (
              SELECT COUNT(*) FROM information_schema.columns
              WHERE table_name = 'hsid_sequences'
                AND column_name IN ('scope', 'next_value')
            ) = 2
            """,
            conn);

        var result = await cmd.ExecuteScalarAsync();
        return result is bool b && b;
    }
}
