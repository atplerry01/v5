using System.Text.Json;
using Confluent.Kafka;
using Npgsql;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Background service that polls the Postgres outbox table and publishes
/// pending events to Kafka. Implements the transactional outbox pattern:
///   1. SELECT ... WHERE status = 'pending' ... FOR UPDATE SKIP LOCKED
///   2. Produce to Kafka using the topic stored in each outbox entry
///   3. UPDATE status = 'published'
///
/// This ensures at-least-once delivery without distributed transactions.
/// Topic is resolved at enqueue time by TopicNameResolver — no hardcoded default.
/// </summary>
public sealed class KafkaOutboxPublisher : BackgroundService
{
    private readonly string _connectionString;
    private readonly IProducer<string, string> _producer;
    private readonly TimeSpan _pollInterval;

    public KafkaOutboxPublisher(
        string connectionString,
        IProducer<string, string> producer,
        TimeSpan? pollInterval = null)
    {
        _connectionString = connectionString;
        _producer = producer;
        _pollInterval = pollInterval ?? TimeSpan.FromSeconds(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var published = await PublishBatchAsync(stoppingToken);
            if (published == 0)
                await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task<int> PublishBatchAsync(CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        // Fetch pending batch with SKIP LOCKED to allow concurrent workers
        await using var selectCmd = new NpgsqlCommand(
            """
            SELECT id, correlation_id, event_type, payload, topic
            FROM outbox
            WHERE status = 'pending'
            ORDER BY created_at ASC
            LIMIT 100
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);

        var batch = new List<(Guid Id, Guid CorrelationId, string EventType, string Payload, string Topic)>();
        await using (var reader = await selectCmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                batch.Add((
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4)));
            }
        }

        if (batch.Count == 0)
        {
            await tx.CommitAsync(ct);
            return 0;
        }

        foreach (var entry in batch)
        {
            var message = new Message<string, string>
            {
                Key = entry.CorrelationId.ToString(),
                Value = entry.Payload,
                Headers = new Headers
                {
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(entry.EventType) },
                    { "correlation-id", System.Text.Encoding.UTF8.GetBytes(entry.CorrelationId.ToString()) }
                }
            };

            await _producer.ProduceAsync(entry.Topic, message, ct);

            await using var updateCmd = new NpgsqlCommand(
                "UPDATE outbox SET status = 'published', published_at = NOW() WHERE id = @id",
                conn, tx);
            updateCmd.Parameters.AddWithValue("id", entry.Id);
            await updateCmd.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
        return batch.Count;
    }

    public override void Dispose()
    {
        _producer.Dispose();
        base.Dispose();
    }
}
