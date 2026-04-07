using System.Text.Json;
using Npgsql;
using Whyce.Runtime.Projection;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Generic Postgres projection table writer (Phase B2b).
/// Configured per-instance with schema name, table name, and aggregate type — domain
/// bootstrap modules construct one writer per projection table they own.
///
/// Behavior preserved from the legacy KafkaProjectionConsumerWorker.WritePostgresProjectionAsync:
///   - INSERT with ON CONFLICT version increment + state replace
///   - aggregate id resolved by reflection on the event's AggregateId property
///   - state serialized from the deserialized event (round-trip is acceptable)
/// </summary>
public sealed class PostgresProjectionWriter : IPostgresProjectionWriter
{
    private readonly string _connectionString;
    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly string _aggregateType;

    public PostgresProjectionWriter(
        string connectionString,
        string schemaName,
        string tableName,
        string aggregateType)
    {
        _connectionString = connectionString;
        _schemaName = schemaName;
        _tableName = tableName;
        _aggregateType = aggregateType;
    }

    public async Task WriteAsync(string eventType, object @event, string correlationId, CancellationToken ct)
    {
        Console.WriteLine($"[PROJECTION] Writing {eventType}");
        var aggregateId = ExtractAggregateId(@event);
        if (aggregateId is null)
        {
            Console.WriteLine($"[PROJECTION] SKIP {eventType}: AggregateId not extractable from {@event.GetType().Name}");
            return;
        }

        var state = JsonSerializer.Serialize(@event, @event.GetType());

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        // Schema/table names cannot be parameterized — they come from constructor config,
        // not from message content, and are validated at bootstrap time.
        var sql = $"""
            INSERT INTO {_schemaName}.{_tableName}
                (aggregate_id, aggregate_type, current_version, state, last_event_type, correlation_id, projected_at, created_at)
            VALUES
                (@aggId, @aggType, 1, @state::jsonb, @eventType, @corrId, NOW(), NOW())
            ON CONFLICT (aggregate_id) DO UPDATE SET
                current_version = {_schemaName}.{_tableName}.current_version + 1,
                state = @state::jsonb,
                last_event_type = @eventType,
                correlation_id = @corrId,
                projected_at = NOW()
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("aggId", aggregateId.Value);
        cmd.Parameters.AddWithValue("aggType", _aggregateType);
        cmd.Parameters.AddWithValue("state", state);
        cmd.Parameters.AddWithValue("eventType", eventType);
        cmd.Parameters.AddWithValue("corrId",
            Guid.TryParse(correlationId, out var cid) ? cid : Guid.Empty);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static Guid? ExtractAggregateId(object @event)
    {
        var prop = @event.GetType().GetProperty("AggregateId");
        if (prop is null) return null;

        var value = prop.GetValue(@event);
        if (value is null) return null;
        if (value is Guid guid) return guid;

        // Value-object fallback: AggregateId / TodoId etc. expose a Value property
        var inner = value.GetType().GetProperty("Value")?.GetValue(value);
        if (inner is Guid g) return g;
        return Guid.TryParse(inner?.ToString() ?? value.ToString(), out var parsed) ? parsed : null;
    }
}
