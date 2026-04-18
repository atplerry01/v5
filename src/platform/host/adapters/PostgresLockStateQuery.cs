using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Postgres-backed <see cref="ILockStateQuery"/>. Reads the active lock
/// projection (schema <c>projection_economic_enforcement_lock</c>, table
/// <c>lock_read_model</c>), filters on the subject and the <c>is_active</c>
/// flag inside the JSON state column, and returns whether the subject is
/// currently locked.
///
/// An active lock is a hard stop — no commands may execute for the subject.
///
/// FAIL-CLOSED POSTURE: a transport or query failure returns
/// <see cref="ActiveLockState.Unavailable"/> (NOT None). Locks are
/// safety-critical — when the system cannot verify lock state, command
/// execution MUST be blocked rather than permitted. This is the opposite
/// of the violation/restriction fail-open posture, because a missed lock
/// bypass is a financial integrity breach.
/// </summary>
public sealed class PostgresLockStateQuery : ILockStateQuery
{
    private const string Sql =
        "SELECT state FROM projection_economic_enforcement_lock.lock_read_model " +
        "WHERE (state->>'subjectId')::uuid = @subject " +
        "  AND (state->>'isActive')::boolean = true " +
        "LIMIT 1";

    private readonly string _connectionString;
    private readonly ILogger<PostgresLockStateQuery>? _logger;

    public PostgresLockStateQuery(
        string connectionString,
        ILogger<PostgresLockStateQuery>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString must be set.", nameof(connectionString));
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<ActiveLockState> QueryBySubjectAsync(
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        if (subjectId == Guid.Empty) return ActiveLockState.None;

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new NpgsqlCommand(Sql, conn);
            cmd.Parameters.AddWithValue("subject", subjectId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var state = reader.GetString(0);
                var model = JsonSerializer.Deserialize<LockReadModel>(state);
                if (model is not null && model.IsActive)
                {
                    return new ActiveLockState(
                        IsLocked: true,
                        IsUnavailable: false,
                        Scope: model.Scope,
                        Reason: model.Reason);
                }
            }

            return ActiveLockState.None;
        }
        catch (Exception ex)
        {
            // FAIL-CLOSED: lock state unknown → block command execution.
            // This is deliberately NOT fail-open. A lock protects financial
            // integrity; permitting execution when lock state is unknown
            // is a safety breach. The detection pipeline will re-verify
            // on the next healthy query.
            _logger?.LogError(ex,
                "PostgresLockStateQuery FAILED for subject {Subject}; FAILING CLOSED — " +
                "command execution will be blocked until lock state can be verified.",
                subjectId);
            return ActiveLockState.Unavailable;
        }
    }
}
