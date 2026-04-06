namespace Whycespace.Domain.IntelligenceSystem.Observability.Metric;

/// <summary>
/// Federation-specific observability metrics.
/// No business logic — observability only.
/// </summary>
public sealed class FederationMetrics
{
    // Graph metrics
    private long _graphBuildCount;
    private long _totalGraphBuildLatencyMs;
    private long _totalGraphSize; // node + edge count

    // Resolution metrics
    private long _resolveCount;
    private long _totalResolveLatencyMs;

    // Incremental update metrics
    private long _incrementalUpdateCount;
    private long _totalIncrementalUpdateLatencyMs;

    // Conflict metrics
    private long _conflictDetectedCount;
    private long _crossClusterConflictCount;

    // Cache metrics
    private long _cacheHitCount;
    private long _cacheMissCount;

    // Diff metrics
    private long _diffComputedCount;
    private long _totalDiffLatencyMs;

    public void RecordGraphBuild(int nodeCount, int edgeCount, long latencyMs)
    {
        Interlocked.Increment(ref _graphBuildCount);
        Interlocked.Add(ref _totalGraphBuildLatencyMs, latencyMs);
        Interlocked.Add(ref _totalGraphSize, nodeCount + edgeCount);
    }

    public void RecordResolve(long latencyMs)
    {
        Interlocked.Increment(ref _resolveCount);
        Interlocked.Add(ref _totalResolveLatencyMs, latencyMs);
    }

    public void RecordIncrementalUpdate(long latencyMs)
    {
        Interlocked.Increment(ref _incrementalUpdateCount);
        Interlocked.Add(ref _totalIncrementalUpdateLatencyMs, latencyMs);
    }

    public void RecordConflictDetected() => Interlocked.Increment(ref _conflictDetectedCount);
    public void RecordCrossClusterConflict() => Interlocked.Increment(ref _crossClusterConflictCount);

    public void RecordCacheHit() => Interlocked.Increment(ref _cacheHitCount);
    public void RecordCacheMiss() => Interlocked.Increment(ref _cacheMissCount);

    public void RecordDiffComputed(long latencyMs)
    {
        Interlocked.Increment(ref _diffComputedCount);
        Interlocked.Add(ref _totalDiffLatencyMs, latencyMs);
    }

    public FederationMetricsSnapshot GetSnapshot() => new()
    {
        GraphBuildCount = Interlocked.Read(ref _graphBuildCount),
        AverageGraphBuildLatencyMs = _graphBuildCount > 0
            ? (double)Interlocked.Read(ref _totalGraphBuildLatencyMs) / Interlocked.Read(ref _graphBuildCount) : 0,
        AverageGraphSize = _graphBuildCount > 0
            ? (double)Interlocked.Read(ref _totalGraphSize) / Interlocked.Read(ref _graphBuildCount) : 0,
        ResolveCount = Interlocked.Read(ref _resolveCount),
        AverageResolveLatencyMs = _resolveCount > 0
            ? (double)Interlocked.Read(ref _totalResolveLatencyMs) / Interlocked.Read(ref _resolveCount) : 0,
        IncrementalUpdateCount = Interlocked.Read(ref _incrementalUpdateCount),
        AverageIncrementalUpdateLatencyMs = _incrementalUpdateCount > 0
            ? (double)Interlocked.Read(ref _totalIncrementalUpdateLatencyMs) / Interlocked.Read(ref _incrementalUpdateCount) : 0,
        ConflictDetectedCount = Interlocked.Read(ref _conflictDetectedCount),
        CrossClusterConflictCount = Interlocked.Read(ref _crossClusterConflictCount),
        CacheHitCount = Interlocked.Read(ref _cacheHitCount),
        CacheMissCount = Interlocked.Read(ref _cacheMissCount),
        DiffComputedCount = Interlocked.Read(ref _diffComputedCount),
        AverageDiffLatencyMs = _diffComputedCount > 0
            ? (double)Interlocked.Read(ref _totalDiffLatencyMs) / Interlocked.Read(ref _diffComputedCount) : 0,
    };
}

public sealed record FederationMetricsSnapshot
{
    public long GraphBuildCount { get; init; }
    public double AverageGraphBuildLatencyMs { get; init; }
    public double AverageGraphSize { get; init; }
    public long ResolveCount { get; init; }
    public double AverageResolveLatencyMs { get; init; }
    public long IncrementalUpdateCount { get; init; }
    public double AverageIncrementalUpdateLatencyMs { get; init; }
    public long ConflictDetectedCount { get; init; }
    public long CrossClusterConflictCount { get; init; }
    public long CacheHitCount { get; init; }
    public long CacheMissCount { get; init; }
    public long DiffComputedCount { get; init; }
    public double AverageDiffLatencyMs { get; init; }

    public double CacheHitRate => (CacheHitCount + CacheMissCount) > 0
        ? (double)CacheHitCount / (CacheHitCount + CacheMissCount) * 100 : 0;

    public double ConflictRate => GraphBuildCount > 0
        ? (double)ConflictDetectedCount / GraphBuildCount * 100 : 0;
}
