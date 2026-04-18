using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Phase 8 B4 — BackgroundService that subscribes to the sanction event
/// topic and routes each envelope through
/// <see cref="SanctionActivationEnforcementHandler"/>. Mirrors
/// <see cref="PayoutFailureCompensationWorker"/> in shape so failure
/// and commit semantics are consistent across the reactor surface.
///
/// Subscribed topic:
///   - whyce.economic.enforcement.sanction.events   (source of SanctionActivatedEvent)
///
/// Dedicated consumer group isolates commit progress from the sanction
/// projection and other consumers. Handler exceptions leave the offset
/// un-committed so the message is redelivered after restart; the
/// handler's envelope-level idempotency claim guarantees a second
/// attempt never produces a duplicate enforcement aggregate.
/// </summary>
public sealed class SanctionActivationEnforcementWorker : BackgroundService
{
    private static readonly string[] Topics =
    [
        "whyce.economic.enforcement.sanction.events"
    ];

    private const string ConsumerGroup = "whyce.saga.sanction-activation-enforcement";

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly SanctionActivationEnforcementHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<SanctionActivationEnforcementWorker>? _logger;

    public SanctionActivationEnforcementWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        SanctionActivationEnforcementHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<SanctionActivationEnforcementWorker>? logger = null)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
        _deserializer = deserializer;
        _handler = handler;
        _clock = clock;
        _consumerOptions = consumerOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            QueuedMaxMessagesKbytes = _consumerOptions.QueuedMaxMessagesKbytes,
            MessageMaxBytes = _consumerOptions.FetchMessageMaxBytes,
            MaxPollIntervalMs = _consumerOptions.MaxPollIntervalMs,
            SessionTimeoutMs = _consumerOptions.SessionTimeoutMs
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topics);

        _logger?.LogInformation(
            "SanctionActivationEnforcementWorker subscribed to {TopicCount} topic(s) under consumer group {Group}.",
            Topics.Length, ConsumerGroup);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null)
                    continue;

                var eventType = ExtractHeader(result.Message.Headers, "event-type");
                var eventIdHeader = ExtractHeader(result.Message.Headers, "event-id");
                var aggregateIdHeader = ExtractHeader(result.Message.Headers, "aggregate-id");
                var correlationHeader = ExtractHeader(result.Message.Headers, "correlation-id");
                var causationHeader = ExtractHeader(result.Message.Headers, "causation-id");

                if (string.IsNullOrEmpty(eventType)
                    || string.IsNullOrEmpty(eventIdHeader)
                    || string.IsNullOrEmpty(aggregateIdHeader)
                    || !Guid.TryParse(eventIdHeader, out var parsedEventId)
                    || !Guid.TryParse(aggregateIdHeader, out var parsedAggregateId))
                {
                    _logger?.LogWarning(
                        "SanctionActivationEnforcementWorker skipping malformed message on {Topic}: missing or unparseable headers.",
                        result.Topic);
                    consumer.Commit(result);
                    continue;
                }

                var payload = _deserializer.DeserializeInbound(eventType, result.Message.Value);

                var envelope = new EventEnvelope
                {
                    EventId = parsedEventId,
                    AggregateId = parsedAggregateId,
                    CorrelationId = Guid.TryParse(correlationHeader, out var cid) ? cid : Guid.Empty,
                    CausationId = Guid.TryParse(causationHeader, out var causId) ? causId : Guid.Empty,
                    EventType = eventType,
                    EventName = eventType,
                    EventVersion = EventVersion.Default,
                    SchemaHash = string.Empty,
                    Payload = payload,
                    ExecutionHash = string.Empty,
                    PolicyHash = string.Empty,
                    Timestamp = _clock.UtcNow
                };

                // Background dispatch has no HTTP context — scope to a
                // known system identity so the downstream dispatcher's
                // identity middleware can satisfy actor / tenant lookups.
                using (SystemIdentityScope.Begin(
                    "system/sanction-activation-enforcement", "system", "system"))
                {
                    await _handler.HandleAsync(envelope, stoppingToken);
                }
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "SanctionActivationEnforcementWorker iteration failed; offset un-committed for redelivery.");
                // Offset is NOT committed — redelivery after restart will
                // retry; the handler's IIdempotencyStore claim ensures a
                // second attempt never produces a duplicate enforcement.
            }
        }

        consumer.Close();
    }

    private static string? ExtractHeader(Headers? headers, string key)
    {
        if (headers is null) return null;
        foreach (var h in headers)
        {
            if (string.Equals(h.Key, key, StringComparison.Ordinal))
            {
                var bytes = h.GetValueBytes();
                return bytes is null ? null : System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
        return null;
    }
}
