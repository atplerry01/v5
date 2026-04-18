using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Postgres-backed <see cref="IRestrictionStateQuery"/>. Reads the active
/// restriction projection (schema <c>projection_economic_enforcement_restriction</c>,
/// table <c>restriction_read_model</c>), filters on the subject and the
/// <c>is_active</c> flag inside the JSON state column, and collapses the
/// result set into a single <see cref="ActiveRestrictionState"/>.
///
/// Any active restriction raises IsRestricted=true with the restriction's
/// scope. Multiple active restrictions collapse to the first match (all
/// restrictions are blocking — scope provides diagnostic context only).
///
/// Failure posture: a transport or query failure is logged at Warning and
/// collapses to <see cref="ActiveRestrictionState.None"/> (fail-open on the
/// hot path).
/// </summary>
public sealed class PostgresRestrictionStateQuery : IRestrictionStateQuery
{
    private const string Sql =
        "SELECT state FROM projection_economic_enforcement_restriction.restriction_read_model " +
        "WHERE (state->>'subjectId')::uuid = @subject " +
        "  AND (state->>'isActive')::boolean = true " +
        "LIMIT 1";

    private readonly string _connectionString;
    private readonly ILogger<PostgresRestrictionStateQuery>? _logger;

    public PostgresRestrictionStateQuery(
        string connectionString,
        ILogger<PostgresRestrictionStateQuery>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString must be set.", nameof(connectionString));
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<ActiveRestrictionState> QueryBySubjectAsync(
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        if (subjectId == Guid.Empty) return ActiveRestrictionState.None;

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
                var model = JsonSerializer.Deserialize<RestrictionReadModel>(state);
                if (model is not null && model.IsActive)
                {
                    return new ActiveRestrictionState(
                        IsRestricted: true,
                        Scope: model.Scope,
                        Reason: model.Reason);
                }
            }

            return ActiveRestrictionState.None;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex,
                "PostgresRestrictionStateQuery failed for subject {Subject}; failing open to no-restriction.",
                subjectId);
            return ActiveRestrictionState.None;
        }
    }
}
