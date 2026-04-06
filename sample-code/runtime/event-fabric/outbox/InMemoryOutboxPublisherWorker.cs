using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;
using TopicResolver = Whycespace.Runtime.Routing.TopicResolver;

namespace Whycespace.Runtime.EventFabric.Outbox;

/// <summary>
/// Drains the InMemoryOutboxStore and publishes events to Kafka.
/// Bridges the RuntimeControlPlane (which writes to the in-memory outbox)
/// to the Kafka event fabric.
///
/// Uses dual-topic model:
///   1. Domain topic (mandatory) — whyce.{cluster}.{subcluster}.{app}.{context}.{event}
///   2. Global topic (incident only) — whyce.{cluster}.{context_leaf}.{event}
/// </summary>
public sealed class InMemoryOutboxPublisherWorker : BackgroundService
{
    private readonly ILogger<InMemoryOutboxPublisherWorker> _logger;
    private readonly IOutboxStore _outboxStore;
    private readonly IMessageProducer _messageProducer;
    private readonly TopicResolver _topicResolver;
    private readonly IClock _clock;
    private readonly TimeSpan _pollInterval;
    private readonly int _batchSize;
    private const int MaxRetries = 5;
    private static readonly TimeSpan PurgeAge = TimeSpan.FromHours(1);
    private DateTimeOffset _lastPurge = DateTimeOffset.MinValue;

    public InMemoryOutboxPublisherWorker(
        ILogger<InMemoryOutboxPublisherWorker> logger,
        IOutboxStore outboxStore,
        IMessageProducer messageProducer,
        TopicResolver topicResolver,
        IClock clock,
        IConfiguration configuration)
    {
        _logger = logger;
        _outboxStore = outboxStore;
        _messageProducer = messageProducer;
        _topicResolver = topicResolver;
        _clock = clock;

        var pollMs = int.TryParse(configuration["OUTBOX_POLL_INTERVAL"], out var p) ? p : 100;
        _pollInterval = TimeSpan.FromMilliseconds(pollMs);
        _batchSize = int.TryParse(configuration["OUTBOX_BATCH_SIZE"], out var b) ? b : 500;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InMemoryOutboxPublisherWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pending = await _outboxStore.GetPendingAsync(_batchSize, stoppingToken);

                if (pending.Count == 0)
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                    continue;
                }

                foreach (var entry in pending)
                {
                    await _outboxStore.MarkProcessingAsync(entry.EntryId, stoppingToken);

                    try
                    {
                        var topics = _topicResolver.ResolveTopics(
                            entry.Event.EventType,
                            entry.Event.Cluster,
                            entry.Event.SubCluster,
                            entry.Event.App,
                            entry.Event.Context);

                        var payload = JsonSerializer.Serialize(new
                        {
                            event_id = entry.Event.EventId,
                            event_type = entry.Event.EventType,
                            aggregate_type = entry.Event.AggregateType,
                            correlation_id = entry.Event.CorrelationId,
                            event_data = entry.Event.Payload,
                            metadata = entry.Event.Headers,
                            timestamp = entry.Event.Timestamp,
                            cluster = entry.Event.Cluster,
                            subcluster = entry.Event.SubCluster,
                            app = entry.Event.App,
                            context = entry.Event.Context
                        });

                        foreach (var topic in topics)
                        {
                            await _messageProducer.PublishAsync(topic, entry.PartitionKey, payload);

                            _logger.LogInformation(
                                "Outbox published {EventId} → {Topic} key={Key}",
                                entry.Event.EventId, topic, entry.PartitionKey);
                        }

                        await _outboxStore.MarkPublishedAsync(entry.EntryId, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to publish outbox entry {EntryId} (retries={Retries})",
                            entry.EntryId, entry.RetryCount);

                        if (entry.RetryCount >= MaxRetries)
                        {
                            await _outboxStore.MarkDeadLetteredAsync(entry.EntryId, stoppingToken);
                            _logger.LogError("Outbox entry {EntryId} moved to DLQ after {MaxRetries} retries",
                                entry.EntryId, MaxRetries);
                        }
                        else
                        {
                            await _outboxStore.MarkFailedAsync(entry.EntryId, ex.Message, stoppingToken);
                        }
                    }
                }

                // Periodic purge of published entries to prevent unbounded growth
                if (_clock.UtcNowOffset - _lastPurge > TimeSpan.FromMinutes(5))
                {
                    await _outboxStore.PurgePublishedAsync(PurgeAge, stoppingToken);
                    _lastPurge = _clock.UtcNowOffset;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InMemoryOutboxPublisherWorker error");
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }

        _logger.LogInformation("InMemoryOutboxPublisherWorker stopped");
    }
}
