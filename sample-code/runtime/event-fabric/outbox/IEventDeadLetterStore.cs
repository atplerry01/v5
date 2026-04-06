using System.Collections.Concurrent;

namespace Whycespace.Runtime.EventFabric.Outbox;

public sealed record EventDeadLetterEntry
{
    public required Guid EventId { get; init; }
    public required string Payload { get; init; }
    public string? Error { get; init; }
    public DateTimeOffset FailedAt { get; init; }
}

/// <summary>
/// Dead letter store for events that failed to publish after max retries.
/// Production: backed by whycespace.event_dlq table.
/// </summary>
public interface IEventDeadLetterStore
{
    Task EnqueueAsync(EventDeadLetterEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDeadLetterEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EventDeadLetterEntry?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
}

public sealed class InMemoryEventDeadLetterStore : IEventDeadLetterStore
{
    private readonly ConcurrentDictionary<Guid, EventDeadLetterEntry> _entries = new();

    public Task EnqueueAsync(EventDeadLetterEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries[entry.EventId] = entry;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EventDeadLetterEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<EventDeadLetterEntry>>(_entries.Values.ToList().AsReadOnly());
    }

    public Task<EventDeadLetterEntry?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        _entries.TryGetValue(eventId, out var entry);
        return Task.FromResult(entry);
    }
}
