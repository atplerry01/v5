using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Background service that polls the Postgres outbox table and publishes
/// pending events to Kafka. Implements the transactional outbox pattern:
///   1. SELECT ... WHERE status = 'pending' ... FOR UPDATE SKIP LOCKED
///   2. Produce to Kafka using the topic stored in each outbox entry
///   3. UPDATE status = 'published' (per row)
///
/// Row-level isolation: a ProduceException on one row marks that row 'failed'
/// and continues with the rest of the batch. The publisher loop NEVER crashes
/// the host — all exceptions are caught and logged.
/// </summary>
public sealed class KafkaOutboxPublisher : BackgroundService
{
    private const int DefaultMaxRetryCount = 5;

    private readonly string _connectionString;
    private readonly IProducer<string, string> _producer;
    private readonly TimeSpan _pollInterval;
    private readonly int _maxRetryCount;
    private readonly ILogger<KafkaOutboxPublisher>? _logger;

    public KafkaOutboxPublisher(
        string connectionString,
        IProducer<string, string> producer,
        TimeSpan? pollInterval = null,
        ILogger<KafkaOutboxPublisher>? logger = null,
        int maxRetryCount = DefaultMaxRetryCount)
    {
        _connectionString = connectionString;
        _producer = producer;
        _pollInterval = pollInterval ?? TimeSpan.FromSeconds(1);
        _maxRetryCount = maxRetryCount;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Outer loop is unconditionally guarded so a transient DB / Kafka
        // failure can never escape and trigger BackgroundServiceExceptionBehavior.StopHost.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published = await PublishBatchAsync(stoppingToken);
                if (published == 0)
                    await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "KafkaOutboxPublisher batch loop error; will retry after delay.");
                try { await Task.Delay(_pollInterval, stoppingToken); }
                catch (OperationCanceledException) { return; }
            }
        }
    }

    private async Task<int> PublishBatchAsync(CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        // Fetch pending + previously-failed-but-under-retry-budget rows with
        // SKIP LOCKED to allow concurrent workers. 'deadletter' rows are excluded.
        await using var selectCmd = new NpgsqlCommand(
            """
            SELECT id, correlation_id, event_type, payload, topic, retry_count
            FROM outbox
            WHERE (status = 'pending')
               OR (status = 'failed' AND retry_count < @max_retry)
            ORDER BY created_at ASC
            LIMIT 100
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        selectCmd.Parameters.AddWithValue("max_retry", _maxRetryCount);

        var batch = new List<(Guid Id, Guid CorrelationId, string EventType, string Payload, string Topic, int RetryCount)>();
        await using (var reader = await selectCmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                batch.Add((
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetInt32(5)));
            }
        }

        if (batch.Count == 0)
        {
            await tx.CommitAsync(ct);
            return 0;
        }

        var publishedCount = 0;

        foreach (var entry in batch)
        {
            // Projection consumer requires event-id + aggregate-id headers (see
            // GenericKafkaProjectionConsumerWorker.ExecuteAsync). The outbox table does
            // not store these as discrete columns, so derive them here:
            //   event-id     ← outbox row id (UUID, unique per outbox entry)
            //   aggregate-id ← payload.AggregateId.Value (or payload.AggregateId if scalar)
            var aggregateId = TryExtractAggregateId(entry.Payload) ?? Guid.Empty;

            var message = new Message<string, string>
            {
                Key = entry.CorrelationId.ToString(),
                Value = entry.Payload,
                Headers = new Headers
                {
                    { "event-id", System.Text.Encoding.UTF8.GetBytes(entry.Id.ToString()) },
                    { "aggregate-id", System.Text.Encoding.UTF8.GetBytes(aggregateId.ToString()) },
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(entry.EventType) },
                    { "correlation-id", System.Text.Encoding.UTF8.GetBytes(entry.CorrelationId.ToString()) }
                }
            };

            try
            {
                _logger?.LogDebug(
                    "Publishing outbox row {OutboxId} ({EventType}) to topic {Topic}",
                    entry.Id, entry.EventType, entry.Topic);

                await _producer.ProduceAsync(entry.Topic, message, ct);
                await MarkAsPublishedAsync(conn, tx, entry.Id, ct);
                publishedCount++;
            }
            catch (ProduceException<string, string> ex)
            {
                _logger?.LogError(
                    ex,
                    "Kafka produce failed for outbox row {OutboxId} on topic {Topic} (attempt {Attempt}/{Max}): {Reason}",
                    entry.Id, entry.Topic, entry.RetryCount + 1, _maxRetryCount, ex.Error.Reason);

                await RecordFailureAsync(conn, tx, entry.Id, entry.RetryCount, ex.Error.Reason, ct);
                continue; // NEVER break the batch on a single bad row.
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Unexpected error publishing outbox row {OutboxId} on topic {Topic} (attempt {Attempt}/{Max})",
                    entry.Id, entry.Topic, entry.RetryCount + 1, _maxRetryCount);

                await RecordFailureAsync(conn, tx, entry.Id, entry.RetryCount, ex.Message, ct);
                continue;
            }
        }

        await tx.CommitAsync(ct);
        return publishedCount;
    }

    private static async Task MarkAsPublishedAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid id, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(
            "UPDATE outbox SET status = 'published', published_at = NOW() WHERE id = @id",
            conn, tx);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task RecordFailureAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid id, int currentRetryCount, string reason, CancellationToken ct)
    {
        // Promote to 'deadletter' once retry budget exhausted; otherwise mark 'failed'
        // (which the next batch will pick up via the SELECT WHERE status='failed' AND retry_count<max).
        var nextStatus = (currentRetryCount + 1) >= _maxRetryCount ? "deadletter" : "failed";

        await using var cmd = new NpgsqlCommand(
            """
            UPDATE outbox
            SET status      = @status,
                retry_count = retry_count + 1,
                last_error  = @err
            WHERE id = @id
            """,
            conn, tx);
        cmd.Parameters.AddWithValue("status", nextStatus);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("err", reason ?? string.Empty);
        await cmd.ExecuteNonQueryAsync(ct);

        if (nextStatus == "deadletter")
        {
            _logger?.LogError(
                "Outbox row {OutboxId} promoted to deadletter after {Max} failed attempts. Last error: {Reason}",
                id, _maxRetryCount, reason);
        }
    }

    /// <summary>
    /// Pulls AggregateId out of the JSONB payload. Tolerates two shapes:
    ///   { "AggregateId": "guid" }
    ///   { "AggregateId": { "Value": "guid" } }
    /// Returns null when the field is absent or unparseable — caller falls back to Guid.Empty.
    /// </summary>
    private static Guid? TryExtractAggregateId(string payloadJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            if (!doc.RootElement.TryGetProperty("AggregateId", out var aggIdElement))
                return null;

            if (aggIdElement.ValueKind == JsonValueKind.String &&
                Guid.TryParse(aggIdElement.GetString(), out var directGuid))
            {
                return directGuid;
            }

            if (aggIdElement.ValueKind == JsonValueKind.Object &&
                aggIdElement.TryGetProperty("Value", out var valueElement) &&
                valueElement.ValueKind == JsonValueKind.String &&
                Guid.TryParse(valueElement.GetString(), out var nestedGuid))
            {
                return nestedGuid;
            }
        }
        catch (JsonException)
        {
            // Malformed payload — fall through to null and let caller use Guid.Empty.
        }
        return null;
    }

    public override void Dispose()
    {
        _producer.Dispose();
        base.Dispose();
    }
}
