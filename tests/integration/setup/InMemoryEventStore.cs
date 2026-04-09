using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// In-memory event store with strict version enforcement.
/// Per aggregate: versions are monotonic, contiguous (no gaps), no duplicates.
/// Append-only — once a version exists, it cannot be overwritten.
///
/// EventOrderingTest exercises this directly.
/// All other integration tests share an instance per TestHost.
/// </summary>
public sealed class InMemoryEventStore : IEventStore
{
    private readonly object _lock = new();
    private readonly Dictionary<Guid, List<Stored>> _store = new();
    private readonly StageRecorder? _recorder;

    public InMemoryEventStore(StageRecorder? recorder = null)
    {
        _recorder = recorder;
    }

    public Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(aggregateId, out var list))
                return Task.FromResult<IReadOnlyList<object>>(Array.Empty<object>());
            return Task.FromResult<IReadOnlyList<object>>(list.Select(e => e.Payload).ToArray());
        }
    }

    public Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<object> events, int expectedVersion, CancellationToken cancellationToken = default)
    {
        if (events.Count == 0) return Task.CompletedTask;

        lock (_lock)
        {
            if (!_store.TryGetValue(aggregateId, out var list))
            {
                list = new List<Stored>();
                _store[aggregateId] = list;
            }

            var nextVersion = list.Count == 0 ? 0 : list[^1].Version + 1;

            for (var i = 0; i < events.Count; i++)
            {
                var version = nextVersion + i;
                if (list.Any(e => e.Version == version))
                    throw new InvalidOperationException(
                        $"Duplicate version {version} for aggregate {aggregateId}. Append-only invariant violated.");

                list.Add(new Stored(version, events[i]));
            }
        }

        _recorder?.Record("Persist");
        return Task.CompletedTask;
    }

    /// <summary>Test-only: returns versions for an aggregate, in append order.</summary>
    public IReadOnlyList<int> Versions(Guid aggregateId)
    {
        lock (_lock)
        {
            return _store.TryGetValue(aggregateId, out var list)
                ? list.Select(e => e.Version).ToArray()
                : Array.Empty<int>();
        }
    }

    /// <summary>Test-only: returns the raw stored event payloads, in append order.</summary>
    public IReadOnlyList<object> AllEvents(Guid aggregateId)
    {
        lock (_lock)
        {
            return _store.TryGetValue(aggregateId, out var list)
                ? list.Select(e => e.Payload).ToArray()
                : Array.Empty<object>();
        }
    }

    private sealed record Stored(int Version, object Payload);
}
