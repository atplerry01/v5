using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Npgsql;
using Whyce.Projections.OperationalSystem.Sandbox.Todo;
using Whyce.Shared.Contracts.Events.Todo;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Background service that consumes domain events from Kafka event topics
/// and dispatches them to projection handlers. This is the missing link
/// between the outbox-published Kafka events and the projection layer.
///
/// Flow: Kafka topic → deserialize → TodoProjectionHandler (Redis)
///                                  → Postgres projection table
///
/// Consumer group: whyce.projection.operational.sandbox.todo
/// Topic: whyce.operational.sandbox.todo.events
/// </summary>
public sealed class KafkaProjectionConsumerWorker : BackgroundService
{
    private readonly string _kafkaBootstrapServers;
    private readonly string _projectionsConnectionString;
    private readonly TodoProjectionHandler _handler;
    private readonly TimeSpan _pollTimeout;

    private const string Topic = "whyce.operational.sandbox.todo.events";
    private const string ConsumerGroup = "whyce.projection.operational.sandbox.todo";

    public KafkaProjectionConsumerWorker(
        string kafkaBootstrapServers,
        string projectionsConnectionString,
        TodoProjectionHandler handler,
        TimeSpan? pollTimeout = null)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
        _projectionsConnectionString = projectionsConnectionString;
        _handler = handler;
        _pollTimeout = pollTimeout ?? TimeSpan.FromSeconds(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(_pollTimeout);
                if (result is null) continue;

                var eventType = ExtractHeader(result.Message.Headers, "event-type");
                var correlationId = ExtractHeader(result.Message.Headers, "correlation-id");
                var payload = result.Message.Value;

                await DispatchToHandlerAsync(eventType, payload);
                await WritePostgresProjectionAsync(eventType, correlationId, payload);

                consumer.Commit(result);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ConsumeException)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        consumer.Close();
    }

    private async Task DispatchToHandlerAsync(string eventType, string payload)
    {
        switch (eventType)
        {
            case "TodoCreatedEvent":
            {
                var raw = JsonSerializer.Deserialize<RawTodoCreatedEvent>(payload);
                if (raw is null) return;
                await _handler.HandleAsync(new TodoCreatedEventSchema(raw.AggregateId.Value, raw.Title));
                break;
            }
            case "TodoUpdatedEvent":
            {
                var raw = JsonSerializer.Deserialize<RawTodoUpdatedEvent>(payload);
                if (raw is null) return;
                await _handler.HandleAsync(new TodoUpdatedEventSchema(raw.AggregateId.Value, raw.Title));
                break;
            }
            case "TodoCompletedEvent":
            {
                var raw = JsonSerializer.Deserialize<RawTodoCompletedEvent>(payload);
                if (raw is null) return;
                await _handler.HandleAsync(new TodoCompletedEventSchema(raw.AggregateId.Value));
                break;
            }
        }
    }

    private async Task WritePostgresProjectionAsync(string eventType, string correlationId, string payload)
    {
        var aggregateId = ExtractAggregateId(payload);
        if (aggregateId is null) return;

        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO projection_operational_sandbox_todo.todo_read_model
                (aggregate_id, aggregate_type, current_version, state, last_event_type, correlation_id, projected_at, created_at)
            VALUES
                (@aggId, 'Todo', 1, @state::jsonb, @eventType, @corrId, NOW(), NOW())
            ON CONFLICT (aggregate_id) DO UPDATE SET
                current_version = projection_operational_sandbox_todo.todo_read_model.current_version + 1,
                state = @state::jsonb,
                last_event_type = @eventType,
                correlation_id = @corrId,
                projected_at = NOW()
            """,
            conn);

        cmd.Parameters.AddWithValue("aggId", aggregateId.Value);
        cmd.Parameters.AddWithValue("state", payload);
        cmd.Parameters.AddWithValue("eventType", eventType);
        cmd.Parameters.AddWithValue("corrId",
            Guid.TryParse(correlationId, out var cid) ? cid : Guid.Empty);

        await cmd.ExecuteNonQueryAsync();
    }

    private static Guid? ExtractAggregateId(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("AggregateId", out var aggProp))
            {
                if (aggProp.ValueKind == JsonValueKind.Object &&
                    aggProp.TryGetProperty("Value", out var valueProp))
                    return valueProp.GetGuid();
                if (aggProp.ValueKind == JsonValueKind.String)
                    return aggProp.GetGuid();
            }
        }
        catch { }
        return null;
    }

    private static string ExtractHeader(Headers headers, string key)
    {
        var header = headers.FirstOrDefault(h => h.Key == key);
        return header is null ? string.Empty : Encoding.UTF8.GetString(header.GetValueBytes());
    }

    // Raw deserialization types matching domain event JSON structure
    private sealed record RawAggregateId(Guid Value);
    private sealed record RawTodoCreatedEvent(RawAggregateId AggregateId, string Title);
    private sealed record RawTodoUpdatedEvent(RawAggregateId AggregateId, string Title);
    private sealed record RawTodoCompletedEvent(RawAggregateId AggregateId);
}
