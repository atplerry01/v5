namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

/// <summary>
/// Phase 3 anomaly detector. Inspects a sequence of latency samples and
/// a time-ordered error log, emits structured anomaly records when:
///
///   * a single latency sample exceeds the running average by
///     <c>LatencySpikeMultiplier</c> (default 3.0), OR
///   * the error count within a rolling <c>ErrorBurstWindow</c> crosses
///     the configured threshold.
///
/// Default thresholds mirror
/// <c>infrastructure/observability/phase3/anomaly-config.json</c> so
/// in-process detection and the production Prometheus alert rules agree
/// by construction. Tests may override via the constructor.
/// </summary>
public sealed class AnomalyDetector
{
    public double LatencySpikeMultiplier { get; }
    public TimeSpan ErrorBurstWindow { get; }
    public int ErrorBurstCount { get; }

    public AnomalyDetector(
        double latencySpikeMultiplier = 3.0,
        TimeSpan? errorBurstWindow = null,
        int errorBurstCount = 3)
    {
        LatencySpikeMultiplier = latencySpikeMultiplier;
        ErrorBurstWindow = errorBurstWindow ?? TimeSpan.FromSeconds(60);
        ErrorBurstCount = errorBurstCount;
    }

    public IReadOnlyList<Anomaly> DetectLatencySpikes(IReadOnlyList<double> latencyMs)
    {
        if (latencyMs.Count == 0) return Array.Empty<Anomaly>();

        // Baseline is the MEDIAN of the remaining samples (per-sample
        // leave-one-out). Using the raw mean would let a single large
        // outlier pull the average up and self-suppress the spike —
        // e.g. samples [10,10,10,10,45,12,11] have mean 15.4 so the
        // 45 looks like only 2.9× the average despite being 4.5× the
        // stable baseline of 10. Median-based detection matches the
        // operational meaning of a spike (a sample that stands out
        // relative to the stable run, not relative to itself).
        var anomalies = new List<Anomaly>();
        var sorted = new double[latencyMs.Count];
        for (var i = 0; i < latencyMs.Count; i++)
        {
            if (latencyMs.Count == 1)
            {
                // single sample — nothing to compare against
                return Array.Empty<Anomaly>();
            }

            var k = 0;
            for (var j = 0; j < latencyMs.Count; j++)
            {
                if (j == i) continue;
                sorted[k++] = latencyMs[j];
            }
            Array.Sort(sorted, 0, k);
            var baseline = k % 2 == 1
                ? sorted[k / 2]
                : (sorted[(k / 2) - 1] + sorted[k / 2]) / 2.0;
            if (baseline <= 0) continue;

            var threshold = baseline * LatencySpikeMultiplier;
            if (latencyMs[i] > threshold)
            {
                anomalies.Add(new Anomaly(
                    Kind: AnomalyKind.LatencySpike,
                    Index: i,
                    Value: latencyMs[i],
                    Threshold: threshold,
                    Detail: $"sample {i}: {latencyMs[i]:F2}ms > {LatencySpikeMultiplier:F1}× baseline ({baseline:F2}ms)"));
            }
        }

        return anomalies;
    }

    public IReadOnlyList<Anomaly> DetectErrorBursts(IReadOnlyList<DateTimeOffset> errorTimes)
    {
        if (errorTimes.Count < ErrorBurstCount) return Array.Empty<Anomaly>();

        var ordered = errorTimes.OrderBy(t => t).ToArray();
        var anomalies = new List<Anomaly>();
        for (var i = 0; i <= ordered.Length - ErrorBurstCount; i++)
        {
            var windowStart = ordered[i];
            var windowEnd = ordered[i + ErrorBurstCount - 1];
            if (windowEnd - windowStart <= ErrorBurstWindow)
            {
                anomalies.Add(new Anomaly(
                    Kind: AnomalyKind.ErrorBurst,
                    Index: i,
                    Value: ErrorBurstCount,
                    Threshold: ErrorBurstCount,
                    Detail: $"{ErrorBurstCount} errors within {(windowEnd - windowStart).TotalSeconds:F1}s " +
                            $"(window={ErrorBurstWindow.TotalSeconds:F0}s) starting at {windowStart:O}"));
                // slide past this burst to avoid N-squared reporting
                i += ErrorBurstCount - 1;
            }
        }

        return anomalies;
    }
}

public enum AnomalyKind
{
    LatencySpike,
    ErrorBurst
}

public sealed record Anomaly(AnomalyKind Kind, int Index, double Value, double Threshold, string Detail);
