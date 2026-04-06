using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed idempotency store. Tracks processed command keys
/// in the idempotency_keys table for duplicate detection.
/// </summary>
public sealed class PostgresIdempotencyStoreAdapter : IIdempotencyStore
{
    private readonly string _connectionString;

    public PostgresIdempotencyStoreAdapter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT 1 FROM idempotency_keys WHERE key = @key LIMIT 1",
            conn);
        cmd.Parameters.AddWithValue("key", key);

        var result = await cmd.ExecuteScalarAsync();
        return result is not null;
    }

    public async Task MarkAsync(string key)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO idempotency_keys (key, created_at)
            VALUES (@key, NOW())
            ON CONFLICT (key) DO NOTHING
            """,
            conn);
        cmd.Parameters.AddWithValue("key", key);

        await cmd.ExecuteNonQueryAsync();
    }
}
