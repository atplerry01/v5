using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Phase 8 B5 — Postgres-backed <see cref="IExpirableSanctionQuery"/>.
/// Reads the active sanction projection (schema
/// <c>projection_economic_enforcement_sanction</c>, table
/// <c>sanction_read_model</c>), filters inside the JSON state column for
/// rows where <c>status = 'Active'</c> AND <c>expiresAt</c> is non-null and
/// at-or-before the supplied <paramref name="now"/>, and returns a bounded
/// batch ordered by the earliest ExpiresAt first so a backlog drains
/// oldest-expiry-first.
///
/// <para>
/// <b>Fail-safe posture.</b> Transport / query failures return an empty
/// candidate list so the scheduler worker treats the tick as "nothing to
/// do" rather than crashing. The next tick re-queries; missed expiries
/// self-heal on the following successful scan. This is deliberately
/// opposite to <see cref="PostgresLockStateQuery"/>'s fail-closed posture
/// — scheduler ticks are not a live safety rail; they drive natural
/// expiry timing.
/// </para>
///
/// <para>
/// <b>Determinism.</b> The query passes <paramref name="now"/> as a bound
/// parameter so the DB's own clock is never consulted — the candidate
/// set is a pure function of (projection state, caller-supplied timestamp).
/// Replay / restart of the scan loop with the same IClock reading
/// produces identical candidates.
/// </para>
/// </summary>
public sealed class PostgresExpirableSanctionQuery : IExpirableSanctionQuery
{
    private const string Sql =
        "SELECT (state->>'sanctionId')::uuid, (state->>'expiresAt')::timestamptz " +
        "FROM projection_economic_enforcement_sanction.sanction_read_model " +
        "WHERE (state->>'status') = 'Active' " +
        "  AND (state->>'expiresAt') IS NOT NULL " +
        "  AND (state->>'expiresAt')::timestamptz <= @now " +
        "ORDER BY (state->>'expiresAt')::timestamptz ASC " +
        "LIMIT @batch";

    private readonly string _connectionString;
    private readonly ILogger<PostgresExpirableSanctionQuery>? _logger;

    public PostgresExpirableSanctionQuery(
        string connectionString,
        ILogger<PostgresExpirableSanctionQuery>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString must be set.", nameof(connectionString));
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExpirableSanctionCandidate>> QueryExpirableAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0) return Array.Empty<ExpirableSanctionCandidate>();

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new NpgsqlCommand(Sql, conn);
            cmd.Parameters.AddWithValue("now", now);
            cmd.Parameters.AddWithValue("batch", batchSize);

            var results = new List<ExpirableSanctionCandidate>(batchSize);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var sanctionId = reader.GetGuid(0);
                var expiresAt = reader.GetFieldValue<DateTimeOffset>(1);
                results.Add(new ExpirableSanctionCandidate(sanctionId, expiresAt));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "PostgresExpirableSanctionQuery scan failed at now={Now}; returning empty candidate batch. " +
                "Next scheduler tick will retry.",
                now);
            return Array.Empty<ExpirableSanctionCandidate>();
        }
    }
}
