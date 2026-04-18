using System.Diagnostics;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

/// <summary>
/// Phase 3 in-process metrics collector. Thread-safe. Records per-dispatch
/// latency (ticks), success / failure counts, and the wall-clock iteration
/// count.
///
/// Intentionally lightweight — the Phase 3 harness does not depend on an
/// external metrics pipeline for its pass/fail decision. The collector
/// produces a <see cref="MetricsSnapshot"/> at the end of each suite /
/// iteration which the tests assert on and which the soak-test harness
/// persists to <c>tests/reports/phase3/metrics-*.json</c> for the report
/// emitter to fold into <c>validation-report.json</c>.
/// </summary>
public sealed class MetricsCollector
{
    private readonly object _lock = new();
    private readonly List<long> _latencyTicks = new();
    private long _successCount;
    private long _failureCount;
    private long _exceptionCount;
    private readonly long _startedTicks = Stopwatch.GetTimestamp();

    public void RecordSuccess(long latencyTicks)
    {
        Interlocked.Increment(ref _successCount);
        AppendLatency(latencyTicks);
    }

    public void RecordFailure(long latencyTicks)
    {
        Interlocked.Increment(ref _failureCount);
        AppendLatency(latencyTicks);
    }

    public void RecordException(long latencyTicks)
    {
        Interlocked.Increment(ref _exceptionCount);
        AppendLatency(latencyTicks);
    }

    private void AppendLatency(long latencyTicks)
    {
        lock (_lock) _latencyTicks.Add(latencyTicks);
    }

    public long SuccessCount => Interlocked.Read(ref _successCount);
    public long FailureCount => Interlocked.Read(ref _failureCount);
    public long ExceptionCount => Interlocked.Read(ref _exceptionCount);

    public MetricsSnapshot Snapshot()
    {
        long[] ticks;
        lock (_lock) ticks = _latencyTicks.ToArray();

        var total = SuccessCount + FailureCount + ExceptionCount;
        var errorCount = FailureCount + ExceptionCount;
        var errorRate = total == 0 ? 0.0 : (double)errorCount / total;

        double avgMs = 0, p50Ms = 0, p95Ms = 0, p99Ms = 0, maxMs = 0;
        if (ticks.Length > 0)
        {
            Array.Sort(ticks);
            var tps = (double)Stopwatch.Frequency;
            double ToMs(long t) => t / tps * 1000.0;

            var sum = 0L;
            for (var i = 0; i < ticks.Length; i++) sum += ticks[i];
            avgMs = ToMs(sum / ticks.Length);

            static int Clamp(int idx, int len) => idx < 0 ? 0 : (idx >= len ? len - 1 : idx);
            p50Ms = ToMs(ticks[Clamp((int)(ticks.Length * 0.50) - 1, ticks.Length)]);
            p95Ms = ToMs(ticks[Clamp((int)(ticks.Length * 0.95) - 1, ticks.Length)]);
            p99Ms = ToMs(ticks[Clamp((int)(ticks.Length * 0.99) - 1, ticks.Length)]);
            maxMs = ToMs(ticks[^1]);
        }

        var elapsedSeconds = (Stopwatch.GetTimestamp() - _startedTicks) / (double)Stopwatch.Frequency;
        var throughputPerSecond = elapsedSeconds <= 0 ? 0 : total / elapsedSeconds;

        return new MetricsSnapshot(
            TotalSamples: (int)total,
            SuccessCount: (int)SuccessCount,
            FailureCount: (int)FailureCount,
            ExceptionCount: (int)ExceptionCount,
            ErrorRate: errorRate,
            AvgMs: avgMs,
            P50Ms: p50Ms,
            P95Ms: p95Ms,
            P99Ms: p99Ms,
            MaxMs: maxMs,
            ThroughputPerSecond: throughputPerSecond,
            ElapsedSeconds: elapsedSeconds);
    }
}

public sealed record MetricsSnapshot(
    int TotalSamples,
    int SuccessCount,
    int FailureCount,
    int ExceptionCount,
    double ErrorRate,
    double AvgMs,
    double P50Ms,
    double P95Ms,
    double P99Ms,
    double MaxMs,
    double ThroughputPerSecond,
    double ElapsedSeconds);
