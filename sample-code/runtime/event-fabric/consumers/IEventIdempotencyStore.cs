namespace Whycespace.Runtime.EventFabric.Consumers;

/// <summary>
/// Persistent idempotency store for event consumption.
/// Unlike the command-level IIdempotencyRegistry, this tracks
/// which events have been processed by downstream consumers.
/// </summary>
public interface IEventIdempotencyStore
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
}

public sealed class InMemoryEventIdempotencyStore : IEventIdempotencyStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, byte> _processed = new();

    public Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_processed.ContainsKey(eventId));
    }

    public Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        _processed.TryAdd(eventId, 0);
        return Task.CompletedTask;
    }
}
