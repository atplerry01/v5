using System.Collections.Concurrent;

namespace Whycespace.Runtime.ControlPlane.CoreSystem;

/// <summary>
/// Deduplicates anomaly events to prevent alert flooding.
/// Uses a sliding window: identical anomalies (same entityId + commandType + reason)
/// within the dedup window are suppressed.
///
/// Thread-safe: ConcurrentDictionary with tick-based expiry.
/// Deterministic: dedup key is a pure function of the anomaly fields.
/// </summary>
public sealed class AnomalyDeduplicator
{
    private readonly ConcurrentDictionary<string, long> _seen = new();
    private readonly TimeSpan _window;

    public AnomalyDeduplicator(TimeSpan window)
    {
        _window = window;
    }

    /// <summary>
    /// Returns true if this anomaly should be emitted (not a duplicate).
    /// Returns false if a matching anomaly was already emitted within the window.
    /// </summary>
    public bool ShouldEmit(CoreSystemAnomaly anomaly)
    {
        var key = BuildKey(anomaly);
        var now = Environment.TickCount64;

        if (_seen.TryGetValue(key, out var lastEmitted))
        {
            if (now - lastEmitted < (long)_window.TotalMilliseconds)
                return false;
        }

        _seen[key] = now;
        return true;
    }

    /// <summary>
    /// Evicts expired dedup entries. Call periodically.
    /// </summary>
    public void EvictExpired()
    {
        var now = Environment.TickCount64;
        var windowMs = (long)_window.TotalMilliseconds;

        foreach (var key in _seen.Keys)
        {
            if (_seen.TryGetValue(key, out var ts) && now - ts >= windowMs)
                _seen.TryRemove(key, out _);
        }
    }

    public int TrackedCount => _seen.Count;

    private static string BuildKey(CoreSystemAnomaly anomaly) =>
        $"{anomaly.EntityId}:{anomaly.CommandType}:{anomaly.Severity}";
}
