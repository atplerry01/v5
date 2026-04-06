using System.Collections.Concurrent;
using Whycespace.Runtime.EventFabric;

namespace Whycespace.Runtime.Persistence;

/// <summary>
/// In-memory event store for testing and development.
/// Thread-safe via ConcurrentDictionary.
/// </summary>
public sealed class EventStore : IEventStore
{
    private readonly ConcurrentDictionary<string, List<RuntimeEvent>> _streams = new();
    private readonly List<RuntimeEvent> _allEvents = [];
    private readonly object _lock = new();

    public Task AppendAsync(string streamId, RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var stream = _streams.GetOrAdd(streamId, _ => []);
            stream.Add(@event);
            _allEvents.Add(@event);
        }
        return Task.CompletedTask;
    }

    public Task AppendAsync(string streamId, IEnumerable<RuntimeEvent> events, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var stream = _streams.GetOrAdd(streamId, _ => []);
            var eventList = events.ToList();
            stream.AddRange(eventList);
            _allEvents.AddRange(eventList);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RuntimeEvent>> ReadStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_streams.TryGetValue(streamId, out var stream))
                return Task.FromResult<IReadOnlyList<RuntimeEvent>>(stream.ToList());
            return Task.FromResult<IReadOnlyList<RuntimeEvent>>([]);
        }
    }

    public Task<IReadOnlyList<RuntimeEvent>> ReadStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_streams.TryGetValue(streamId, out var stream))
                return Task.FromResult<IReadOnlyList<RuntimeEvent>>(stream.Skip((int)fromVersion).ToList());
            return Task.FromResult<IReadOnlyList<RuntimeEvent>>([]);
        }
    }

    public Task<IReadOnlyList<RuntimeEvent>> ReadAllAsync(DateTimeOffset? after = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var events = after.HasValue
                ? _allEvents.Where(e => e.Timestamp > after.Value).ToList()
                : _allEvents.ToList();
            return Task.FromResult<IReadOnlyList<RuntimeEvent>>(events);
        }
    }

    public Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_streams.TryGetValue(streamId, out var stream))
                return Task.FromResult((long)stream.Count);
            return Task.FromResult(0L);
        }
    }
}
