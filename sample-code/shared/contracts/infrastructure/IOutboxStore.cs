namespace Whycespace.Shared.Contracts.Infrastructure;

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
    public Guid EntryId { get; init; } = Guid.Empty;
    public required StoredEvent Event { get; init; }
    public required string PartitionKey { get; init; }
    public OutboxEntryStatus Status { get; set; } = OutboxEntryStatus.Pending;
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Outbox store contract for transactional event publishing.
/// Events are first persisted to the outbox, then asynchronously
/// published to the message bus by a background worker.
/// </summary>
public interface IOutboxStore
{
    Task AppendAsync(StoredEvent @event, string partitionKey, CancellationToken cancellationToken = default);
    Task AppendAsync(IEnumerable<StoredEvent> events, string partitionKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkProcessingAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task MarkPublishedAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(Guid entryId, string error, CancellationToken cancellationToken = default);
    Task MarkDeadLetteredAsync(Guid entryId, CancellationToken cancellationToken = default);
}
