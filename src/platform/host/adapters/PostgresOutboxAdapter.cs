using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed outbox. Events are persisted to the outbox table for
/// reliable at-least-once delivery. The KafkaOutboxPublisher polls this
/// table and relays events to Kafka.
/// </summary>
public sealed class PostgresOutboxAdapter : IOutbox
{
    private readonly string _connectionString;

    public PostgresOutboxAdapter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events, string topic)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        for (var i = 0; i < events.Count; i++)
        {
            var domainEvent = events[i];
            var eventType = domainEvent.GetType().Name;
            var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            var idempotencyKey = ComputeIdempotencyKey(correlationId, payload, i);
            var eventId = ComputeDeterministicId(correlationId, eventType, i);
            var aggregateId = ExtractAggregateId(domainEvent);

            await using var cmd = new NpgsqlCommand(
                """
                INSERT INTO outbox (id, correlation_id, event_id, aggregate_id, event_type, payload, idempotency_key, topic, status, created_at)
                VALUES (@id, @corrId, @eventId, @aggregateId, @evtType, @payload::jsonb, @idempKey, @topic, 'pending', NOW())
                ON CONFLICT (idempotency_key) DO NOTHING
                """,
                conn, tx);

            cmd.Parameters.AddWithValue("id", eventId);
            cmd.Parameters.AddWithValue("corrId", correlationId);
            cmd.Parameters.AddWithValue("eventId", eventId);
            cmd.Parameters.AddWithValue("aggregateId", aggregateId);
            cmd.Parameters.AddWithValue("evtType", eventType);
            cmd.Parameters.AddWithValue("payload", payload);
            cmd.Parameters.AddWithValue("idempKey", idempotencyKey);
            cmd.Parameters.AddWithValue("topic", topic);

            await cmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
    }

    private static Guid ComputeDeterministicId(Guid correlationId, string eventType, int sequenceNumber)
    {
        var seed = $"{correlationId}:{eventType}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }

    /// <summary>
    /// Pulls AggregateId off the strongly-typed domain event by convention.
    /// All domain events in this codebase carry an `AggregateId` property of
    /// type <see cref="AggregateId"/> (or, defensively, a raw <see cref="Guid"/>).
    /// Returns Guid.Empty when neither shape is present — the column is NOT NULL
    /// in the outbox schema, so we never return null.
    /// </summary>
    private static Guid ExtractAggregateId(object domainEvent)
    {
        var prop = domainEvent.GetType().GetProperty("AggregateId");
        if (prop is null) return Guid.Empty;

        var value = prop.GetValue(domainEvent);
        return value switch
        {
            AggregateId aid => aid.Value,
            Guid g => g,
            _ => Guid.Empty
        };
    }

    private static string ComputeIdempotencyKey(Guid correlationId, string payload, int sequenceNumber)
    {
        var seed = $"{correlationId}:{payload}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(hash);
    }
}
