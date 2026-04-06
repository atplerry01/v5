namespace Whycespace.Runtime.EventFabric.Outbox;

public enum OutboxEntryStatus
{
    Pending,
    Processing,
    Published,
    Failed,
    DeadLettered
}

public sealed record OutboxEntry
{
    public Guid EntryId { get; init; }
    public required RuntimeEvent Event { get; init; }
    public required string PartitionKey { get; init; }
    public OutboxEntryStatus Status { get; set; } = OutboxEntryStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Outbox store for transactional event publishing.
/// Events are first persisted to the outbox, then asynchronously
/// published to the event fabric (e.g. Kafka) by a background worker.
///
/// Production implementations must use SELECT ... FOR UPDATE SKIP LOCKED
/// to ensure safe concurrent worker operation.
/// </summary>
public interface IOutboxStore
{
    Task AppendAsync(RuntimeEvent @event, string partitionKey, CancellationToken cancellationToken = default);
    Task AppendAsync(IEnumerable<RuntimeEvent> events, string partitionKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkProcessingAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task MarkPublishedAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(Guid entryId, string error, CancellationToken cancellationToken = default);
    Task MarkDeadLetteredAsync(Guid entryId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Purges published entries older than the specified age.
    /// Prevents unbounded growth of the outbox table/store.
    /// </summary>
    Task PurgePublishedAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
}
