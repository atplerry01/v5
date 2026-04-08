using System.Text.Json;
using Npgsql;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Kernel.Domain;

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
    private readonly IIdGenerator _idGenerator;

    public PostgresEventStoreAdapter(string connectionString, EventDeserializer deserializer, IIdGenerator idGenerator)
    {
        _connectionString = connectionString;
        _deserializer = deserializer;
        _idGenerator = idGenerator;
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

        // phase1-gate-H8a: per-aggregate exclusive advisory lock. Two-key form
        // namespaces the lock to ('events', aggregate_id) so it cannot collide
        // with locks taken by other adapters on the same Postgres instance.
        // Auto-released on COMMIT or ROLLBACK. Concurrent appends to the SAME
        // aggregate serialize here; appends to DIFFERENT aggregates run in
        // parallel. This closes the SELECT MAX(version) → INSERT TOCTOU race
        // that the previous Read Committed implementation relied on the PK
        // collision to (accidentally) catch.
        await using (var lockCmd = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(hashtext('events'), hashtext(@agg::text))",
            conn, tx))
        {
            lockCmd.Parameters.AddWithValue("agg", aggregateId);
            await lockCmd.ExecuteNonQueryAsync();
        }

        // Compute current max version for this aggregate inside the transaction.
        // Combined with the H8a advisory lock above, this is the linearization
        // point for per-aggregate version assignment.
        int currentMax;
        await using (var maxCmd = new NpgsqlCommand(
            "SELECT COALESCE(MAX(version), -1) FROM events WHERE aggregate_id = @id",
            conn, tx))
        {
            maxCmd.Parameters.AddWithValue("id", aggregateId);
            var scalar = await maxCmd.ExecuteScalarAsync();
            currentMax = scalar is int v ? v : Convert.ToInt32(scalar);
        }

        // phase1-gate-H8b: optimistic concurrency enforcement.
        // Sentinel -1 means "no check" — preserves the prior behavior for any
        // caller that has not yet been migrated to assert a version. When the
        // caller DOES assert a positive version and it disagrees with what the
        // store actually has, we throw a named exception BEFORE the INSERT so
        // nothing is persisted and the transaction rolls back cleanly.
        if (expectedVersion != -1 && expectedVersion != currentMax)
        {
            throw new ConcurrencyConflictException(aggregateId, expectedVersion, currentMax);
        }

        // phase1-gate-H7-H9-safe (#8): single multi-row INSERT instead of one
        // command per event. Same INSERT semantics, same parameters, same
        // determinism — collapses N round-trips into 1.
        if (events.Count > 0)
        {
            var sql = new System.Text.StringBuilder(
                "INSERT INTO events (id, aggregate_id, aggregate_type, event_type, payload, version, created_at) VALUES ");

            await using var cmd = new NpgsqlCommand { Connection = conn, Transaction = tx };

            for (var i = 0; i < events.Count; i++)
            {
                var version = currentMax + i + 1;
                var domainEvent = events[i];
                var eventType = domainEvent.GetType().Name;
                var aggregateType = ExtractAggregateType(domainEvent);
                var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

                if (i > 0) sql.Append(", ");
                sql.Append($"(@id{i}, @agg{i}, @aggType{i}, @evtType{i}, @payload{i}::jsonb, @ver{i}, NOW())");

                cmd.Parameters.AddWithValue($"id{i}", _idGenerator.Generate($"{aggregateId}:{version}"));
                cmd.Parameters.AddWithValue($"agg{i}", aggregateId);
                cmd.Parameters.AddWithValue($"aggType{i}", aggregateType);
                cmd.Parameters.AddWithValue($"evtType{i}", eventType);
                cmd.Parameters.AddWithValue($"payload{i}", payload);
                cmd.Parameters.AddWithValue($"ver{i}", version);
            }

            cmd.CommandText = sql.ToString();
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
