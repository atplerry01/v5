using System.Collections.Concurrent;

namespace Whycespace.Runtime.ControlPlane.CoreSystem;

/// <summary>
/// Aggregates anomalies within a time window before emitting a summary.
/// Reduces individual anomaly emissions to a single aggregated report
/// per window, preventing write amplification on the anomaly store.
///
/// Thread-safe: ConcurrentDictionary with periodic flush.
/// </summary>
public sealed class AnomalyAggregator
{
    private readonly ConcurrentDictionary<string, AnomalyBucket> _buckets = new();
    private readonly TimeSpan _aggregationWindow;

    public AnomalyAggregator(TimeSpan aggregationWindow)
    {
        _aggregationWindow = aggregationWindow;
    }

    /// <summary>
    /// Records an anomaly into the aggregation bucket.
    /// </summary>
    public void Record(CoreSystemAnomaly anomaly)
    {
        var key = $"{anomaly.CommandType}:{anomaly.Severity}";
        _buckets.AddOrUpdate(
            key,
            _ => new AnomalyBucket(anomaly.CommandType, anomaly.Severity, 1, Environment.TickCount64),
            (_, existing) => existing.Increment());
    }

    /// <summary>
    /// Flushes all buckets that have exceeded the aggregation window.
    /// Returns the aggregated summaries for emission.
    /// </summary>
    public IReadOnlyList<AnomalySummary> Flush()
    {
        var now = Environment.TickCount64;
        var windowMs = (long)_aggregationWindow.TotalMilliseconds;
        var summaries = new List<AnomalySummary>();

        foreach (var key in _buckets.Keys)
        {
            if (_buckets.TryGetValue(key, out var bucket) && now - bucket.WindowStart >= windowMs)
            {
                if (_buckets.TryRemove(key, out var removed))
                {
                    summaries.Add(new AnomalySummary
                    {
                        CommandType = removed.CommandType,
                        Severity = removed.Severity,
                        Count = removed.Count,
                        WindowDuration = _aggregationWindow
                    });
                }
            }
        }

        return summaries;
    }

    public int ActiveBuckets => _buckets.Count;
}

public sealed record AnomalyBucket(string CommandType, AnomalySeverity Severity, int Count, long WindowStart)
{
    public AnomalyBucket Increment() => this with { Count = Count + 1 };
}

public sealed record AnomalySummary
{
    public required string CommandType { get; init; }
    public required AnomalySeverity Severity { get; init; }
    public required int Count { get; init; }
    public required TimeSpan WindowDuration { get; init; }
}
