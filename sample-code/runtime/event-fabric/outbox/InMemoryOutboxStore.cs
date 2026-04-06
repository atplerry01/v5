using System.Collections.Concurrent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.EventFabric.Outbox;

/// <summary>
/// In-memory outbox store with simulated SKIP LOCKED semantics for testing.
/// Entries marked as Processing are excluded from GetPendingAsync to prevent
/// duplicate pickup by concurrent workers.
/// </summary>
public sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, OutboxEntry> _entries = new();
    private readonly object _pendingLock = new();
    private readonly IClock _clock;

    public InMemoryOutboxStore(IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;
    }

    public Task AppendAsync(RuntimeEvent @event, string partitionKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        var entry = new OutboxEntry { EntryId = DeterministicIdHelper.FromSeed($"outbox:{@event.EventId}"), Event = @event, PartitionKey = partitionKey, CreatedAt = _clock.UtcNowOffset };
        _entries[entry.EntryId] = entry;
        return Task.CompletedTask;
    }

    public Task AppendAsync(IEnumerable<RuntimeEvent> events, string partitionKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        foreach (var @event in events)
        {
            var entry = new OutboxEntry { EntryId = DeterministicIdHelper.FromSeed($"outbox:{@event.EventId}"), Event = @event, PartitionKey = partitionKey, CreatedAt = _clock.UtcNowOffset };
            _entries[entry.EntryId] = entry;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns pending entries, excluding those currently being processed (SKIP LOCKED simulation).
    /// </summary>
    public Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        lock (_pendingLock)
        {
            var pending = _entries.Values
                .Where(e => e.Status == OutboxEntryStatus.Pending)
                .OrderBy(e => e.CreatedAt)
                .Take(batchSize)
                .ToList();

            return Task.FromResult<IReadOnlyList<OutboxEntry>>(pending);
        }
    }

    public Task MarkProcessingAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(entryId, out var entry))
        {
            entry.Status = OutboxEntryStatus.Processing;
            entry.LastAttemptAt = _clock.UtcNowOffset;
        }
        return Task.CompletedTask;
    }

    public Task MarkPublishedAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(entryId, out var entry))
        {
            entry.Status = OutboxEntryStatus.Published;
            entry.PublishedAt = _clock.UtcNowOffset;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks entry as failed and reverts to Pending for retry.
    /// The OutboxWorker is responsible for checking retry count
    /// and calling MarkDeadLetteredAsync when retries are exhausted.
    /// </summary>
    public Task MarkFailedAsync(Guid entryId, string error, CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(entryId, out var entry))
        {
            entry.RetryCount++;
            entry.Error = error;
            entry.LastAttemptAt = _clock.UtcNowOffset;
            // Revert to Pending so the entry gets re-picked on next poll
            entry.Status = OutboxEntryStatus.Pending;
        }
        return Task.CompletedTask;
    }

    public Task MarkDeadLetteredAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(entryId, out var entry))
        {
            entry.Status = OutboxEntryStatus.DeadLettered;
        }
        return Task.CompletedTask;
    }

    public Task PurgePublishedAsync(TimeSpan olderThan, CancellationToken cancellationToken = default)
    {
        var cutoff = _clock.UtcNowOffset - olderThan;
        var toPurge = _entries.Values
            .Where(e => e.Status == OutboxEntryStatus.Published && e.PublishedAt < cutoff)
            .Select(e => e.EntryId)
            .ToList();

        foreach (var id in toPurge)
            _entries.TryRemove(id, out _);

        return Task.CompletedTask;
    }

    // Test helpers
    public IReadOnlyList<OutboxEntry> GetAll() => _entries.Values.ToList().AsReadOnly();

    public OutboxEntry? GetById(Guid entryId) =>
        _entries.TryGetValue(entryId, out var entry) ? entry : null;
}
