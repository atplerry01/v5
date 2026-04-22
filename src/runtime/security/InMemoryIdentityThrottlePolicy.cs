using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Trust;

namespace Whycespace.Runtime.Security;

/// <summary>
/// Sliding-window in-memory throttle policy for identity operations.
/// Tracks failed attempt timestamps per key and refuses when the count within
/// the window exceeds the configured maximum. Not suitable for multi-instance
/// deployments — replace with a Redis-backed implementation for production clusters.
/// </summary>
public sealed class InMemoryIdentityThrottlePolicy : IIdentityThrottlePolicy
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _window;
    private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> _attempts = new();

    public InMemoryIdentityThrottlePolicy(int maxAttempts = 5, TimeSpan? window = null)
    {
        _maxAttempts = maxAttempts;
        _window = window ?? TimeSpan.FromMinutes(15);
    }

    public Task<bool> IsThrottledAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_attempts.TryGetValue(key, out var queue))
            return Task.FromResult(false);

        lock (queue)
        {
            Prune(queue);
            return Task.FromResult(queue.Count >= _maxAttempts);
        }
    }

    public Task RecordFailedAttemptAsync(string key, CancellationToken cancellationToken = default)
    {
        var queue = _attempts.GetOrAdd(key, _ => new Queue<DateTimeOffset>());
        lock (queue)
        {
            Prune(queue);
            queue.Enqueue(DateTimeOffset.UtcNow);
        }
        return Task.CompletedTask;
    }

    public Task ResetAsync(string key, CancellationToken cancellationToken = default)
    {
        _attempts.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private void Prune(Queue<DateTimeOffset> queue)
    {
        var cutoff = DateTimeOffset.UtcNow - _window;
        while (queue.Count > 0 && queue.Peek() < cutoff)
            queue.Dequeue();
    }
}
