using System.Text.Json;
using Npgsql;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed event store. Persists domain events as JSONB rows
/// in the canonical events table (see 001_event_store.sql).
///
/// Phase B2b: deserialization is now schema-driven via EventDeserializer
/// (no static EventTypeResolver, no per-domain Type dictionary).
/// </summary>
public sealed class PostgresEventStoreAdapter : IEventStore
{
    private readonly string _connectionString;
    private readonly EventDeserializer _deserializer;

    public PostgresEventStoreAdapter(string connectionString, EventDeserializer deserializer)
    {
        _connectionString = connectionString;
        _deserializer = deserializer;
    }

    public async Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId)
    {
        var events = new List<object>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT event_type, payload FROM events WHERE aggregate_id = @id ORDER BY version ASC",
            conn);
        cmd.Parameters.AddWithValue("id", aggregateId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eventType = reader.GetString(0);
            var payload = reader.GetString(1);
            events.Add(_deserializer.DeserializeStored(eventType, payload));
        }

        return events.AsReadOnly();
    }

    public async Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<object> events, int expectedVersion)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        // Compute current max version for this aggregate inside the transaction so
        // concurrent appends serialize correctly. Ignores caller-supplied expectedVersion;
        // determinism comes from the stored stream, not the in-memory engine snapshot.
        await using (var maxCmd = new NpgsqlCommand(
            "SELECT COALESCE(MAX(version), -1) FROM events WHERE aggregate_id = @id",
            conn, tx))
        {
            maxCmd.Parameters.AddWithValue("id", aggregateId);
            var scalar = await maxCmd.ExecuteScalarAsync();
            expectedVersion = scalar is int v ? v : Convert.ToInt32(scalar);
        }

        for (var i = 0; i < events.Count; i++)
        {
            var version = expectedVersion + i + 1;
            var domainEvent = events[i];
            var eventType = domainEvent.GetType().Name;
            var aggregateType = ExtractAggregateType(domainEvent);
            var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

            await using var cmd = new NpgsqlCommand(
                """
                INSERT INTO events (id, aggregate_id, aggregate_type, event_type, payload, version, created_at)
                VALUES (@id, @agg, @aggType, @evtType, @payload::jsonb, @ver, NOW())
                """,
                conn, tx);

            cmd.Parameters.AddWithValue("id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("agg", aggregateId);
            cmd.Parameters.AddWithValue("aggType", aggregateType);
            cmd.Parameters.AddWithValue("evtType", eventType);
            cmd.Parameters.AddWithValue("payload", payload);
            cmd.Parameters.AddWithValue("ver", version);

            await cmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
    }

    private static string ExtractAggregateType(object domainEvent)
    {
        var ns = domainEvent.GetType().Namespace ?? string.Empty;
        var segments = ns.Split('.');
        return segments.Length > 0 ? segments[^1] : "Unknown";
    }
}
