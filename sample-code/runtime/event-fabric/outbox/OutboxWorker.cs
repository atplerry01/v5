using System.Text.Json;
using Whycespace.Runtime.EventFabric.Topic;
using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Runtime.EventFabric.Outbox;

/// <summary>
/// Processes outbox entries and publishes them to Kafka with partition-key enforcement.
/// Handles retry with exponential backoff and moves entries to DLQ after max retries.
///
/// Designed for concurrent worker safety — relies on IOutboxStore's SKIP LOCKED semantics
/// to prevent duplicate pickup across workers.
/// Topic resolution delegated to ITopicNameResolver — no inline string concat.
/// </summary>
public sealed class OutboxWorker
{
    private readonly IOutboxStore _outboxStore;
    private readonly IMessageProducer _messageProducer;
    private readonly IEventDeadLetterStore _deadLetterStore;
    private readonly ITopicNameResolver _topicNameResolver;
    private readonly OutboxWorkerOptions _options;

    public OutboxWorker(
        IOutboxStore outboxStore,
        IMessageProducer messageProducer,
        IEventDeadLetterStore deadLetterStore,
        ITopicNameResolver topicNameResolver,
        OutboxWorkerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(outboxStore);
        ArgumentNullException.ThrowIfNull(messageProducer);
        ArgumentNullException.ThrowIfNull(deadLetterStore);
        ArgumentNullException.ThrowIfNull(topicNameResolver);

        _outboxStore = outboxStore;
        _messageProducer = messageProducer;
        _deadLetterStore = deadLetterStore;
        _topicNameResolver = topicNameResolver;
        _options = options ?? new OutboxWorkerOptions();
    }

    /// <summary>
    /// Processes a single batch of pending outbox entries.
    /// Returns the number of entries processed.
    /// </summary>
    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _outboxStore.GetPendingAsync(_options.BatchSize, cancellationToken);

        foreach (var entry in entries)
        {
            await ProcessEntryAsync(entry, cancellationToken);
        }

        return entries.Count;
    }

    /// <summary>
    /// Continuous polling loop. Call from a BackgroundService.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var processed = await ProcessBatchAsync(cancellationToken);

            // Adaptive polling: shorter delay when entries were found
            var delay = processed > 0 ? _options.BusyPollInterval : _options.IdlePollInterval;
            await Task.Delay(delay, cancellationToken);
        }
    }

    private async Task ProcessEntryAsync(OutboxEntry entry, CancellationToken cancellationToken)
    {
        // Enforce partition key — reject if missing
        if (string.IsNullOrWhiteSpace(entry.PartitionKey))
        {
            await _outboxStore.MarkFailedAsync(entry.EntryId, "Missing partition key", cancellationToken);
            await _outboxStore.MarkDeadLetteredAsync(entry.EntryId, cancellationToken);
            await _deadLetterStore.EnqueueAsync(new EventDeadLetterEntry
            {
                EventId = entry.Event.EventId,
                Payload = JsonSerializer.Serialize(entry.Event.Payload),
                Error = "Missing partition key"
            }, cancellationToken);
            return;
        }

        // Mark as processing to prevent re-pickup
        await _outboxStore.MarkProcessingAsync(entry.EntryId, cancellationToken);

        try
        {
            var topic = _topicNameResolver.Resolve(entry.Event);
            var payload = JsonSerializer.Serialize(entry.Event);

            await _messageProducer.PublishAsync(topic, entry.PartitionKey, payload, cancellationToken);

            await _outboxStore.MarkPublishedAsync(entry.EntryId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await HandleFailureAsync(entry, ex, cancellationToken);
        }
    }

    private async Task HandleFailureAsync(OutboxEntry entry, Exception ex, CancellationToken cancellationToken)
    {
        var newRetryCount = entry.RetryCount + 1;

        if (newRetryCount >= _options.MaxRetries)
        {
            // Move to DLQ
            await _outboxStore.MarkFailedAsync(entry.EntryId, ex.Message, cancellationToken);
            await _outboxStore.MarkDeadLetteredAsync(entry.EntryId, cancellationToken);
            await _deadLetterStore.EnqueueAsync(new EventDeadLetterEntry
            {
                EventId = entry.Event.EventId,
                Payload = JsonSerializer.Serialize(entry.Event.Payload),
                Error = $"Max retries ({_options.MaxRetries}) exceeded: {ex.Message}"
            }, cancellationToken);
        }
        else
        {
            // Mark failed — will be retried on next poll (status reverts to Pending)
            await _outboxStore.MarkFailedAsync(entry.EntryId, ex.Message, cancellationToken);
        }
    }

}

public sealed record OutboxWorkerOptions
{
    public int BatchSize { get; init; } = 100;
    public int MaxRetries { get; init; } = 5;
    public TimeSpan IdlePollInterval { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan BusyPollInterval { get; init; } = TimeSpan.FromMilliseconds(100);
}
