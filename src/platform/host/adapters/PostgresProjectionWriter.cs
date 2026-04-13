using System.Text.Json;
using Npgsql;
using Whycespace.Runtime.Projection;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Generic Postgres projection table writer.
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
    private readonly ProjectionsDataSource _dataSource;
    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly string _aggregateType;

    // phase1.5-S5.2.2 / KC-4 (PROJECTIONS-POOL-01): connection
    // lifecycle moved to the declared projections pool. Acquisitions
    // flow through PostgresPoolMetrics.OpenInstrumentedAsync with
    // pool="projections" tag. Query logic, ON CONFLICT semantics, and
    // upsert behavior are unchanged.
    public PostgresProjectionWriter(
        ProjectionsDataSource dataSource,
        string schemaName,
        string tableName,
        string aggregateType)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
        _schemaName = schemaName;
        _tableName = tableName;
        _aggregateType = aggregateType;
    }

    public async Task WriteAsync(string eventType, object @event, string correlationId, CancellationToken ct)
    {
        // phase1.6-S2.1: silent skip when AggregateId is not extractable.
        // The generic writer is a fallback for raw-payload projections; the
        // canonical observability surface for skip/write events is the
        // ProjectionDispatcher metrics (PROJECTION_INVOKED_COUNTER), not
        // ad-hoc console output.
        var aggregateId = ExtractAggregateId(@event);
        if (aggregateId is null) return;

        var state = JsonSerializer.Serialize(@event, @event.GetType());

        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(ProjectionsDataSource.PoolName, ct);

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
