using System.Collections.Concurrent;
using System.Text.Json;

namespace Whycespace.Runtime.Persistence;

/// <summary>
/// In-memory projection store for testing and development.
/// Thread-safe via ConcurrentDictionary.
/// </summary>
public sealed class ProjectionStore : IProjectionStore
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _data = new();
    private readonly ConcurrentDictionary<string, long> _checkpoints = new();

    public Task<T?> GetAsync<T>(string projectionName, string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_data.TryGetValue(projectionName, out var projection) &&
            projection.TryGetValue(key, out var json))
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string projectionName, string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        var projection = _data.GetOrAdd(projectionName, _ => new ConcurrentDictionary<string, string>());
        projection[key] = JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string projectionName, string key, CancellationToken cancellationToken = default)
    {
        if (_data.TryGetValue(projectionName, out var projection))
            projection.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<T>> GetAllAsync<T>(string projectionName, CancellationToken cancellationToken = default) where T : class
    {
        if (_data.TryGetValue(projectionName, out var projection))
        {
            var items = projection.Values
                .Select(json => JsonSerializer.Deserialize<T>(json)!)
                .ToList();
            return Task.FromResult<IReadOnlyList<T>>(items);
        }
        return Task.FromResult<IReadOnlyList<T>>([]);
    }

    public Task<long> GetCheckpointAsync(string projectionName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_checkpoints.GetValueOrDefault(projectionName, 0L));
    }

    public Task SetCheckpointAsync(string projectionName, long position, CancellationToken cancellationToken = default)
    {
        _checkpoints[projectionName] = position;
        return Task.CompletedTask;
    }
}
