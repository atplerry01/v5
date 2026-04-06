using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Messaging;

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

            await using var cmd = new NpgsqlCommand(
                """
                INSERT INTO outbox (id, correlation_id, event_type, payload, idempotency_key, topic, status, created_at)
                VALUES (@id, @corrId, @evtType, @payload::jsonb, @idempKey, @topic, 'pending', NOW())
                ON CONFLICT (idempotency_key) DO NOTHING
                """,
                conn, tx);

            cmd.Parameters.AddWithValue("id", ComputeDeterministicId(correlationId, eventType, i));
            cmd.Parameters.AddWithValue("corrId", correlationId);
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

    private static string ComputeIdempotencyKey(Guid correlationId, string payload, int sequenceNumber)
    {
        var seed = $"{correlationId}:{payload}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(hash);
    }
}
