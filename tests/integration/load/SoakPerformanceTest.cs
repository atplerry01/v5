using System.Diagnostics;
using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Load;

/// <summary>
/// Phase 1.5B / §5.3.2 — Compressed Soak Test.
///
/// Drives the canonical Todo command path through the real
/// <c>RuntimeControlPlane</c> at the §5.3.1 baseline rate (100 RPS)
/// for a compressed 60-second soak window, then computes stage-wise
/// (T0–T20, T20–T40, T40–T60) latency percentiles and resource
/// counters so latency / memory / GC drift is observable across the
/// window. The §5.3.1 baseline is the comparison point.
///
/// SCOPE NOTE: this is a COMPRESSED soak (60 s instead of the
/// canonical ≥60 min) per the §5.3.2 prompt. Long-duration soak
/// remains explicitly out of scope for this Phase 1.5B step.
///
/// STRICT CONSTRAINTS (per §5.3.2 prompt)
///   - Zero src/ production code modifications.
///   - Zero new instrumentation. Only existing meters / process counters.
///   - Real execution against TestHost.ForTodo() — same composition
///     as the §5.3.1 baseline so the comparison is apples-to-apples.
///   - Gated by env var `SoakTest__Enabled=true`. Silently skipped
///     otherwise.
/// </summary>
public sealed class SoakPerformanceTest
{
    private const int TargetRps = 100;
    private const int DurationSeconds = 60;
    private const int StageCount = 3; // T0–T20, T20–T40, T40–T60
    private const int StageSeconds = DurationSeconds / StageCount;

    private static bool SoakEnabled() =>
        string.Equals(
            Environment.GetEnvironmentVariable("SoakTest__Enabled"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    [Fact]
    public async Task Soak_100_Rps_For_60_Seconds_Drift_Profile()
    {
        if (!SoakEnabled()) return;

        var host = TestHost.ForTodo();

        // Per-stage latency buffers, pre-sized so the measurement loop
        // never resizes mid-run.
        var perStageLatenciesUs = new List<long>[StageCount];
        for (var i = 0; i < StageCount; i++)
        {
            perStageLatenciesUs[i] = new List<long>(TargetRps * StageSeconds + 64);
        }

        var process = Process.GetCurrentProcess();

        // Stage boundary snapshots: index 0 = T0 (start), 1 = T20,
        // 2 = T40, 3 = T60 (end). All non-latency counters are
        // captured at the boundaries; latency is bucketed per stage.
        var wsAt = new long[StageCount + 1];
        var cpuMsAt = new double[StageCount + 1];
        var gc0At = new int[StageCount + 1];
        var gc1At = new int[StageCount + 1];
        var gc2At = new int[StageCount + 1];

        var failed = 0L;
        var exceptions = 0L;
        var dispatched = 0L;

        void Snapshot(int slot, double cpuMsBaseline)
        {
            wsAt[slot] = Environment.WorkingSet;
            cpuMsAt[slot] = process.TotalProcessorTime.TotalMilliseconds - cpuMsBaseline;
            gc0At[slot] = GC.CollectionCount(0);
            gc1At[slot] = GC.CollectionCount(1);
            gc2At[slot] = GC.CollectionCount(2);
        }

        var cpuMsBaseline = process.TotalProcessorTime.TotalMilliseconds;
        Snapshot(0, cpuMsBaseline);

        var perRequestBudgetTicks = Stopwatch.Frequency / TargetRps;
        var runStopwatch = Stopwatch.StartNew();
        var nextDeadlineTicks = runStopwatch.ElapsedTicks;
        var stopAtTicks = Stopwatch.Frequency * (long)DurationSeconds;
        var nextStageBoundaryTicks = Stopwatch.Frequency * (long)StageSeconds;
        var currentStage = 0;

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

            // Stage rollover: capture the boundary snapshot before
            // we attribute any further requests to the next stage.
            if (currentStage < StageCount - 1
                && runStopwatch.ElapsedTicks >= nextStageBoundaryTicks)
            {
                Snapshot(currentStage + 1, cpuMsBaseline);
                currentStage++;
                nextStageBoundaryTicks += Stopwatch.Frequency * (long)StageSeconds;
            }

            var aggregateId = Guid.NewGuid();
            var ctx = host.NewTodoContext(aggregateId);

            var rqStart = Stopwatch.GetTimestamp();
            try
            {
                var result = await host.ControlPlane.ExecuteAsync(
                    new CreateTodoCommand(aggregateId, $"soak-{aggregateId}"),
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
            perStageLatenciesUs[currentStage].Add(elapsedUs);

            dispatched++;
            nextDeadlineTicks += perRequestBudgetTicks;
        }

        runStopwatch.Stop();
        Snapshot(StageCount, cpuMsBaseline);

        var elapsedSeconds = runStopwatch.Elapsed.TotalSeconds;
        var actualRps = dispatched / Math.Max(1.0, elapsedSeconds);

        var stagePctl = new (long P50, long P95, long P99, long Max, int N)[StageCount];
        for (var s = 0; s < StageCount; s++)
        {
            var arr = perStageLatenciesUs[s].ToArray();
            Array.Sort(arr);
            stagePctl[s] = (
                Percentile(arr, 0.50),
                Percentile(arr, 0.95),
                Percentile(arr, 0.99),
                arr.Length > 0 ? arr[^1] : 0,
                arr.Length);
        }

        // Diagnostic line — captured verbatim into the §5.3.2 evidence
        // file.
        Console.WriteLine(
            $"[§5.3.2 soak harness] target={TargetRps}rps×{DurationSeconds}s stages={StageCount} " +
            $"dispatched={dispatched} failed={failed} exceptions={exceptions} " +
            $"actualRps={actualRps:F1} elapsed={elapsedSeconds:F2}s");
        for (var s = 0; s < StageCount; s++)
        {
            Console.WriteLine(
                $"[§5.3.2 soak stage {s} t={s * StageSeconds}-{(s + 1) * StageSeconds}s] " +
                $"n={stagePctl[s].N} " +
                $"latencyUs(p50/p95/p99/max)={stagePctl[s].P50}/{stagePctl[s].P95}/{stagePctl[s].P99}/{stagePctl[s].Max} " +
                $"wsStart={wsAt[s] / 1024 / 1024}MB wsEnd={wsAt[s + 1] / 1024 / 1024}MB " +
                $"cpuMs={(cpuMsAt[s + 1] - cpuMsAt[s]):F0} " +
                $"gc(0/1/2)={gc0At[s + 1] - gc0At[s]}/{gc1At[s + 1] - gc1At[s]}/{gc2At[s + 1] - gc2At[s]}");
        }

        // ── Acceptance criteria (S1–S6) ──

        // S1 — no crash / instability.
        Assert.Equal(0L, exceptions);

        // S5 — no hidden errors / retries (failed count recorded
        // explicitly).
        Assert.Equal(0L, failed);

        // S4 — throughput stable inside ±10%.
        var lowerBound = TargetRps * 0.90;
        var upperBound = TargetRps * 1.10;
        Assert.True(actualRps >= lowerBound && actualRps <= upperBound,
            $"S4 throughput out of tolerance: actualRps={actualRps:F1} target={TargetRps}");

        // S2 — latency drift bound: each stage's p99 must be within
        // 5× the first stage's p99 (a generous bound that catches
        // catastrophic degradation while tolerating tail noise from a
        // single GC pause). Also assert sortedness inside each stage.
        for (var s = 0; s < StageCount; s++)
        {
            Assert.True(stagePctl[s].P50 <= stagePctl[s].P95);
            Assert.True(stagePctl[s].P95 <= stagePctl[s].P99);
            Assert.True(stagePctl[s].N > 0, $"S2 stage {s} captured zero samples");
        }
        var firstStageP99 = Math.Max(1, stagePctl[0].P99);
        for (var s = 1; s < StageCount; s++)
        {
            Assert.True(stagePctl[s].P99 <= firstStageP99 * 5,
                $"S2 latency drift: stage {s} p99={stagePctl[s].P99} > 5× stage 0 p99={firstStageP99}");
        }

        // S3 — memory growth bounded: end working set must be within
        // 3× the start working set across the 60s window. The in-memory
        // composition accumulates events / outbox batches by design,
        // so a strict equality is not appropriate; the bound catches
        // a runaway leak rather than steady accumulation.
        var wsStartMb = wsAt[0] / 1024 / 1024;
        var wsEndMb = wsAt[StageCount] / 1024 / 1024;
        Assert.True(wsEndMb <= wsStartMb * 3,
            $"S3 memory growth unbounded: start={wsStartMb}MB end={wsEndMb}MB");

        // S6 — reproducibility is structural: gated, fixed cadence,
        // single-dispatcher. No assertion needed beyond the gate.
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
