using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric.Topic;
using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Runtime.EventFabric.Outbox;

/// <summary>
/// Publishes pending outbox entries via IOutboxStore + IMessageProducer.
/// No direct SQL — all persistence through IOutboxStore contract.
/// Topic resolution delegated to ITopicNameResolver — no inline string concat.
/// </summary>
public sealed class OutboxPublisherWorker : BackgroundService
{
    private const int MaxRetries = 5;

    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IOutboxStore _outboxStore;
    private readonly IMessageProducer _messageProducer;
    private readonly ITopicNameResolver _topicNameResolver;
    private readonly int _batchSize;
    private readonly TimeSpan _pollInterval;

    public OutboxPublisherWorker(
        ILogger<OutboxPublisherWorker> logger,
        IConfiguration configuration,
        IOutboxStore outboxStore,
        IMessageProducer messageProducer,
        ITopicNameResolver topicNameResolver)
    {
        _logger = logger;
        _outboxStore = outboxStore;
        _messageProducer = messageProducer;
        _topicNameResolver = topicNameResolver;

        _batchSize = int.TryParse(configuration["OUTBOX_BATCH_SIZE"], out var bs) ? bs : 100;
        _pollInterval = TimeSpan.FromMilliseconds(
            int.TryParse(configuration["OUTBOX_POLL_INTERVAL"], out var pi) ? pi : 1000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxPublisherWorker started — batch={BatchSize}, poll={PollMs}ms",
            _batchSize, _pollInterval.TotalMilliseconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published = await ProcessBatchAsync(stoppingToken);

                if (published == 0)
                    await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisherWorker error during batch processing");
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }

        _logger.LogInformation("OutboxPublisherWorker stopped");
    }

    private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var entries = await _outboxStore.GetPendingAsync(_batchSize, cancellationToken);
        if (entries.Count == 0)
            return 0;

        var published = 0;

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.PartitionKey))
            {
                _logger.LogError("Outbox entry {EntryId} missing partition_key — sending to DLQ", entry.EntryId);
                await _outboxStore.MarkFailedAsync(entry.EntryId, "Missing partition_key", cancellationToken);
                await _outboxStore.MarkDeadLetteredAsync(entry.EntryId, cancellationToken);
                continue;
            }

            await _outboxStore.MarkProcessingAsync(entry.EntryId, cancellationToken);

            try
            {
                var topic = _topicNameResolver.Resolve(entry.Event);
                var payload = JsonSerializer.Serialize(entry.Event);
                await _messageProducer.PublishAsync(topic, entry.PartitionKey, payload);
                await _outboxStore.MarkPublishedAsync(entry.EntryId, cancellationToken);
                published++;

                _logger.LogDebug(
                    "Outbox published {EntryId} → {Topic} key={Key}",
                    entry.EntryId, topic, entry.PartitionKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex, "Failed to publish outbox entry {EntryId} (retries={Retries})",
                    entry.EntryId, entry.RetryCount);

                if (entry.RetryCount + 1 >= MaxRetries)
                {
                    await _outboxStore.MarkFailedAsync(entry.EntryId, $"Max retries ({MaxRetries}) exceeded: {ex.Message}", cancellationToken);
                    await _outboxStore.MarkDeadLetteredAsync(entry.EntryId, cancellationToken);
                    _logger.LogError("Outbox entry {EntryId} moved to DLQ", entry.EntryId);
                }
                else
                {
                    await _outboxStore.MarkFailedAsync(entry.EntryId, ex.Message, cancellationToken);
                }
            }
        }

        _logger.LogInformation("Outbox batch complete: {Published}/{Total} published", published, entries.Count);
        return published;
    }
}
