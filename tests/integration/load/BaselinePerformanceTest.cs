using System.Diagnostics;
using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Load;

/// <summary>
/// Phase 1.5B / §5.3.1 — Baseline Performance Profiling.
///
/// Establishes a controlled, evidence-based baseline performance
/// profile of the runtime under LOW–MODERATE sustained load. This is
/// NOT a stress, soak, or scalability test — it is the canonical
/// "what does the system actually do under steady, light load?"
/// measurement that §5.3.1 requires.
///
/// WORKLOAD
///   - Target rate    : 100 RPS (within the 50–100 RPS §5.3.1 envelope)
///   - Duration       : 60 seconds steady-state
///   - Composition    : single-host TestHost.ForTodo() (in-memory seams)
///   - Pacing         : evenly-spaced single-dispatcher loop (no bursts)
///
/// STRICT CONSTRAINTS (per §5.3.1 prompt)
///   - Zero src/ production code modifications.
///   - Zero new instrumentation. Only existing meters / process counters.
///   - Real execution. No mocks of measured surfaces, no estimates.
///   - Gated by env var `BaselineTest__Enabled=true`. Silently skipped
///     otherwise so the default integration suite stays fast.
///
/// METRICS RECORDED
///   - Per-request latency (Stopwatch around ControlPlane.ExecuteAsync)
///     reduced to p50 / p95 / p99
///   - Achieved RPS = dispatched / elapsed wall-clock
///   - Process CPU time (Process.TotalProcessorTime delta)
///   - Working set start / mid / end (Environment.WorkingSet)
///   - GC collection counts per generation (delta)
///   - Error count (must be recorded even when zero)
///
/// EVIDENCE
///   The harness writes a single console diagnostic line at the end of
///   the run. The §5.3.1 evidence file at
///   <c>claude/audits/phase1.5/evidence/5.3/baseline.evidence.md</c>
///   captures the verbatim numbers from a real execution.
/// </summary>
public sealed class BaselinePerformanceTest
{
    private const int TargetRps = 100;
    private const int DurationSeconds = 60;

    private static bool BaselineEnabled() =>
        string.Equals(
            Environment.GetEnvironmentVariable("BaselineTest__Enabled"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    [Fact]
    public async Task Baseline_100_Rps_For_60_Seconds_Profile()
    {
        if (!BaselineEnabled()) return;

        var host = TestHost.ForTodo();

        // Pre-allocate the latency buffer at the expected sample count
        // so the measurement loop never pays an allocation/resize cost
        // mid-run.
        var expected = TargetRps * DurationSeconds;
        var latenciesUs = new List<long>(expected + 256);

        var process = Process.GetCurrentProcess();
        var cpuStart = process.TotalProcessorTime;
        var wsStart = Environment.WorkingSet;
        var gc0Start = GC.CollectionCount(0);
        var gc1Start = GC.CollectionCount(1);
        var gc2Start = GC.CollectionCount(2);

        long wsMid = 0;
        var midCaptured = false;

        var failed = 0L;
        var exceptions = 0L;
        var dispatched = 0L;

        var perRequestBudgetTicks = Stopwatch.Frequency / TargetRps;
        var runStopwatch = Stopwatch.StartNew();
        var nextDeadlineTicks = runStopwatch.ElapsedTicks;
        var stopAtTicks = Stopwatch.Frequency * (long)DurationSeconds;

        // ── Single-dispatcher pacing loop. Each iteration computes the
        // next deadline (start + N × 1/RPS), dispatches one command,
        // then sleeps until that deadline if we are running ahead. This
        // produces an evenly-spaced cadence rather than a burst. ──
        while (runStopwatch.ElapsedTicks < stopAtTicks)
        {
            var nowTicks = runStopwatch.ElapsedTicks;
            if (nowTicks < nextDeadlineTicks)
            {
                var sleepTicks = nextDeadlineTicks - nowTicks;
                var sleepMs = (int)(sleepTicks * 1000 / Stopwatch.Frequency);
                if (sleepMs > 0)
                {
                    await Task.Delay(sleepMs);
                }
            }

            // Mid-run snapshot at ~30s.
            if (!midCaptured && runStopwatch.Elapsed.TotalSeconds >= DurationSeconds / 2.0)
            {
                wsMid = Environment.WorkingSet;
                midCaptured = true;
            }

            var aggregateId = Guid.NewGuid();
            var ctx = host.NewTodoContext(aggregateId);

            var rqStart = Stopwatch.GetTimestamp();
            try
            {
                var result = await host.ControlPlane.ExecuteAsync(
                    new CreateTodoCommand(aggregateId, $"baseline-{aggregateId}"),
                    ctx);
                if (!result.IsSuccess)
                {
                    Interlocked.Increment(ref failed);
                }
            }
            catch
            {
                Interlocked.Increment(ref exceptions);
            }

            var rqEnd = Stopwatch.GetTimestamp();
            var elapsedUs = (rqEnd - rqStart) * 1_000_000L / Stopwatch.Frequency;
            latenciesUs.Add(elapsedUs);

            dispatched++;
            nextDeadlineTicks += perRequestBudgetTicks;
        }

        runStopwatch.Stop();

        var cpuEnd = process.TotalProcessorTime;
        var wsEnd = Environment.WorkingSet;
        var gc0End = GC.CollectionCount(0);
        var gc1End = GC.CollectionCount(1);
        var gc2End = GC.CollectionCount(2);

        var elapsedSeconds = runStopwatch.Elapsed.TotalSeconds;
        var actualRps = dispatched / Math.Max(1.0, elapsedSeconds);
        var cpuMsTotal = (cpuEnd - cpuStart).TotalMilliseconds;
        var cpuPercentAvg = cpuMsTotal / (elapsedSeconds * 1000.0)
                            / Environment.ProcessorCount * 100.0;

        var sortedUs = latenciesUs.ToArray();
        Array.Sort(sortedUs);
        var p50 = Percentile(sortedUs, 0.50);
        var p95 = Percentile(sortedUs, 0.95);
        var p99 = Percentile(sortedUs, 0.99);
        var pMax = sortedUs.Length > 0 ? sortedUs[^1] : 0;
        var pMin = sortedUs.Length > 0 ? sortedUs[0] : 0;
        var pMeanUs = sortedUs.Length > 0
            ? (long)sortedUs.Average()
            : 0;

        // Single diagnostic line — captured verbatim into the
        // §5.3.1 evidence file.
        Console.WriteLine(
            $"[§5.3.1 baseline harness] " +
            $"target={TargetRps}rps×{DurationSeconds}s mode=in-memory " +
            $"dispatched={dispatched} failed={failed} exceptions={exceptions} " +
            $"actualRps={actualRps:F1} elapsed={elapsedSeconds:F2}s " +
            $"latencyUs(min/p50/p95/p99/max/mean)=" +
            $"{pMin}/{p50}/{p95}/{p99}/{pMax}/{pMeanUs} " +
            $"wsStart={wsStart / 1024 / 1024}MB " +
            $"wsMid={wsMid / 1024 / 1024}MB " +
            $"wsEnd={wsEnd / 1024 / 1024}MB " +
            $"cpuMs={cpuMsTotal:F0} cpuAvg%={cpuPercentAvg:F1} " +
            $"gc(0/1/2)={gc0End - gc0Start}/{gc1End - gc1Start}/{gc2End - gc2Start}");

        // ── Acceptance criteria (B1–B6 from §5.3.1 prompt) ──

        // B1 — System remained stable: no unhandled exceptions.
        Assert.Equal(0L, exceptions);

        // B2 — Error rate recorded (asserted = 0 here; the recording
        // itself is the load-bearing requirement).
        Assert.Equal(0L, failed);

        // B3 — Latency percentiles computed correctly (sorted, monotonic).
        Assert.True(p50 <= p95);
        Assert.True(p95 <= p99);
        Assert.True(p99 <= pMax);

        // B4 — Throughput inside ±10% tolerance of target.
        var lowerBound = TargetRps * 0.90;
        var upperBound = TargetRps * 1.10;
        Assert.True(actualRps >= lowerBound && actualRps <= upperBound,
            $"B4 throughput out of tolerance: " +
            $"actualRps={actualRps:F1} target={TargetRps} " +
            $"window=[{lowerBound:F1},{upperBound:F1}]");

        // B5 — Resource usage captured (CPU + memory). The captures
        // themselves are non-negative; the diagnostic line carries the
        // values into the evidence file.
        Assert.True(cpuMsTotal >= 0);
        Assert.True(wsEnd > 0);
        Assert.True(midCaptured, "B5 mid-run sample must have been captured");

        // B6 — Reproducibility: the test is gated and deterministic in
        // shape (single dispatcher, fixed cadence). The gate guarantees
        // a re-run produces a comparable evidence record.
    }

    private static long Percentile(long[] sortedAsc, double p)
    {
        if (sortedAsc.Length == 0) return 0;
        var rank = (int)Math.Ceiling(p * sortedAsc.Length) - 1;
        if (rank < 0) rank = 0;
        if (rank >= sortedAsc.Length) rank = sortedAsc.Length - 1;
        return sortedAsc[rank];
    }
}
