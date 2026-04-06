using System.Collections.Concurrent;

namespace Whycespace.Shared.Contracts.Engine;

/// <summary>
/// Memoizing decorator for IValidationGate. Caches validation results
/// for a configurable TTL to avoid repeated identical checks within
/// the same execution window.
///
/// Thread-safe: uses ConcurrentDictionary with lazy value factory.
/// Deterministic: same (commandType, entityId) always produces same result
/// within the cache window. Cache is time-bounded, not unbounded.
///
/// Cache key: "{commandType}:{entityId}"
/// </summary>
public sealed class CachedValidationGate : IValidationGate
{
    private readonly IValidationGate _inner;
    private readonly TimeSpan _ttl;
    private readonly ConcurrentDictionary<string, CachedEntry> _cache = new();

    public CachedValidationGate(IValidationGate inner, TimeSpan ttl)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
        _ttl = ttl;
    }

    public async Task<ValidationResult> ValidateAsync(
        string commandType, string entityId, CancellationToken cancellationToken = default)
    {
        var key = $"{commandType}:{entityId}";

        if (_cache.TryGetValue(key, out var cached) && !cached.IsExpired)
            return cached.Result;

        var result = await _inner.ValidateAsync(commandType, entityId, cancellationToken);
        _cache[key] = new CachedEntry(result, _ttl);
        return result;
    }

    /// <summary>
    /// Evicts expired entries. Call periodically from a background timer
    /// to prevent unbounded memory growth.
    /// </summary>
    public void EvictExpired()
    {
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.IsExpired)
                _cache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Clears the entire cache. Use after state-changing operations
    /// that invalidate previous validation results.
    /// </summary>
    public void Invalidate() => _cache.Clear();

    public int CacheSize => _cache.Count;

    private sealed class CachedEntry
    {
        public ValidationResult Result { get; }
        private readonly long _expiresAtTicks;

        public CachedEntry(ValidationResult result, TimeSpan ttl)
        {
            Result = result;
            _expiresAtTicks = Environment.TickCount64 + (long)ttl.TotalMilliseconds;
        }

        public bool IsExpired => Environment.TickCount64 >= _expiresAtTicks;
    }
}
