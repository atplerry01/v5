using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Postgres-backed <see cref="IViolationStateQuery"/>. Reads the active
/// violation projection (schema <c>projection_economic_enforcement_violation</c>,
/// table <c>violation_read_model</c>), filters on the subject reference and
/// the <c>is_active</c> flag inside the JSON state column, and collapses the
/// result set into a single <see cref="ActiveViolationState"/>.
///
/// Priority: any active row with <c>action = "Block"</c> raises IsBlocked;
/// otherwise an active Restrict row raises the Restrict constraint. All
/// other active states (Warn / Escalate) are surfaced as the raw action
/// string so handlers may degrade per-case without re-querying.
///
/// Failure posture: a transport or query failure is logged at Warning and
/// collapses to <see cref="ActiveViolationState.None"/> (fail-open on the
/// hot path). Correctness is owned by the projection + detection pipeline,
/// not by this read-through guard.
/// </summary>
public sealed class PostgresViolationStateQuery : IViolationStateQuery
{
    private const string Sql =
        "SELECT state FROM projection_economic_enforcement_violation.violation_read_model " +
        "WHERE (state->>'sourceReference')::uuid = @subject " +
        "  AND (state->>'isActive')::boolean = true";

    private readonly string _connectionString;
    private readonly ILogger<PostgresViolationStateQuery>? _logger;

    public PostgresViolationStateQuery(
        string connectionString,
        ILogger<PostgresViolationStateQuery>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString must be set.", nameof(connectionString));
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<ActiveViolationState> QueryBySubjectAsync(
        Guid subjectReference,
        CancellationToken cancellationToken = default)
    {
        if (subjectReference == Guid.Empty) return ActiveViolationState.None;

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new NpgsqlCommand(Sql, conn);
            cmd.Parameters.AddWithValue("subject", subjectReference);

            string? restrictConstraint = null;
            string? otherAction = null;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var state = reader.GetString(0);
                var model = JsonSerializer.Deserialize<ViolationReadModel>(state);
                if (model is null) continue;

                if (string.Equals(model.Action, "Block", StringComparison.OrdinalIgnoreCase))
                    return new ActiveViolationState(IsBlocked: true, Constraint: "Block");

                if (string.Equals(model.Action, "Restrict", StringComparison.OrdinalIgnoreCase))
                    restrictConstraint = "Restrict";
                else if (otherAction is null && !string.IsNullOrEmpty(model.Action))
                    otherAction = model.Action;
            }

            if (restrictConstraint is not null)
                return new ActiveViolationState(IsBlocked: false, Constraint: restrictConstraint);
            if (otherAction is not null)
                return new ActiveViolationState(IsBlocked: false, Constraint: otherAction);

            return ActiveViolationState.None;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex,
                "PostgresViolationStateQuery failed for subject {Subject}; failing open to no-constraint.",
                subjectReference);
            return ActiveViolationState.None;
        }
    }
}
