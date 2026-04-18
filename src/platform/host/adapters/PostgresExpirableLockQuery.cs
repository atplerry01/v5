using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Phase 8 B5 — Postgres-backed <see cref="IExpirableLockQuery"/>. Reads
/// the lock projection (schema <c>projection_economic_enforcement_lock</c>,
/// table <c>lock_read_model</c>), filters inside the JSON state column for
/// rows where <c>status = 'Locked'</c> AND <c>expiresAt</c> is non-null and
/// at-or-before the supplied <paramref name="now"/>. Shape parity with
/// <see cref="PostgresExpirableSanctionQuery"/>.
///
/// <para>
/// <b>Suspended locks.</b> The <c>status = 'Locked'</c> filter is the
/// authoritative exclusion rule — <c>LockAggregate.Suspend</c> transitions
/// status to <c>Suspended</c>, so a paused-timer lock is naturally
/// outside the candidate set until it is Resumed. Checking the
/// projection's <c>suspendedAt</c> column would be redundant and would
/// mis-handle resumed locks (which still carry the historical
/// suspendedAt stamp).
/// </para>
/// </summary>
public sealed class PostgresExpirableLockQuery : IExpirableLockQuery
{
    private const string Sql =
        "SELECT (state->>'lockId')::uuid, (state->>'expiresAt')::timestamptz " +
        "FROM projection_economic_enforcement_lock.lock_read_model " +
        "WHERE (state->>'status') = 'Locked' " +
        "  AND (state->>'expiresAt') IS NOT NULL " +
        "  AND (state->>'expiresAt')::timestamptz <= @now " +
        "ORDER BY (state->>'expiresAt')::timestamptz ASC " +
        "LIMIT @batch";

    private readonly string _connectionString;
    private readonly ILogger<PostgresExpirableLockQuery>? _logger;

    public PostgresExpirableLockQuery(
        string connectionString,
        ILogger<PostgresExpirableLockQuery>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString must be set.", nameof(connectionString));
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExpirableLockCandidate>> QueryExpirableAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0) return Array.Empty<ExpirableLockCandidate>();

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new NpgsqlCommand(Sql, conn);
            cmd.Parameters.AddWithValue("now", now);
            cmd.Parameters.AddWithValue("batch", batchSize);

            var results = new List<ExpirableLockCandidate>(batchSize);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var lockId = reader.GetGuid(0);
                var expiresAt = reader.GetFieldValue<DateTimeOffset>(1);
                results.Add(new ExpirableLockCandidate(lockId, expiresAt));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "PostgresExpirableLockQuery scan failed at now={Now}; returning empty candidate batch. " +
                "Next scheduler tick will retry.",
                now);
            return Array.Empty<ExpirableLockCandidate>();
        }
    }
}
