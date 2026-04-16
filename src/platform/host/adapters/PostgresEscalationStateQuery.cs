using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Enforcement;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Postgres-backed <see cref="IEscalationStateQuery"/>. Reads the escalation
/// projection (schema <c>projection_economic_enforcement_escalation</c>,
/// table <c>escalation_read_model</c>) keyed by the subject PK. Fails open
/// to <see cref="ActiveEscalationState.None"/> on transport error.
/// </summary>
public sealed class PostgresEscalationStateQuery : IEscalationStateQuery
{
    private const string Sql =
        "SELECT state FROM projection_economic_enforcement_escalation.escalation_read_model " +
        "WHERE aggregate_id = @subject LIMIT 1";

    private readonly string _connectionString;
    private readonly ILogger<PostgresEscalationStateQuery>? _logger;

    public PostgresEscalationStateQuery(
        string connectionString,
        ILogger<PostgresEscalationStateQuery>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString must be set.", nameof(connectionString));
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<ActiveEscalationState> QueryBySubjectAsync(
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        if (subjectId == Guid.Empty) return ActiveEscalationState.None;

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new NpgsqlCommand(Sql, conn);
            cmd.Parameters.AddWithValue("subject", subjectId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return ActiveEscalationState.None;

            var state = reader.GetString(0);
            var model = JsonSerializer.Deserialize<EscalationReadModel>(state);
            if (model is null) return ActiveEscalationState.None;

            return new ActiveEscalationState(
                Level: model.EscalationLevel,
                Count: model.ViolationCount,
                SeverityScore: model.SeverityScore);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex,
                "PostgresEscalationStateQuery failed for subject {Subject}; failing open to None.",
                subjectId);
            return ActiveEscalationState.None;
        }
    }
}
