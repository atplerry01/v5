using System.Collections.Concurrent;
using System.Diagnostics;
using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Load;

/// <summary>
/// Phase 1.5B / §5.3.3 — Stress / Saturation Test.
///
/// Drives the canonical Todo command path through the real
/// <c>RuntimeControlPlane</c> at progressively increasing target
/// RPS (100 → 500 → 1,000 → 2,000), 20 seconds per stage, then
/// records per-stage throughput / latency / resource curves so the
/// saturation point, degradation curve, and first failure mode are
/// observable in a single run.
///
/// SCOPE NOTE: this is a stress / saturation curve, NOT soak. The
/// stages run back-to-back inside a single test invocation against
/// a single <c>TestHost.ForTodo()</c> instance — we're characterising
/// the runtime's response to increasing load on this host, not
/// re-validating §5.3.1 baseline or §5.3.2 soak behavior.
///
/// STRICT CONSTRAINTS (per §5.3.3 prompt)
///   - Zero src/ production code modifications.
///   - Zero new instrumentation. Only existing meters / process counters.
///   - Real execution against TestHost.ForTodo() — same composition as
///     §5.3.1 / §5.3.2 so the comparison is structural.
///   - Gated by env var `StressTest__Enabled=true`. Silently skipped
///     otherwise.
///
/// CRITICAL DETECTION POINTS
///   The harness records every stage but flags any of the following
///   in the diagnostic line and the evidence file:
///     - error rate > 1%
///     - p99 latency > 10× the baseline (stage 0) p99
///     - throughput collapse (actual RPS < 50% of target RPS)
///   The test still completes all four stages so the full curve is
///   captured even after the first instability — escalation is NOT
///   short-circuited at runtime; characterisation is the goal.
/// </summary>
public sealed class StressPerformanceTest
{
    private const int StageSeconds = 20;
    private const int WorkerCount = 16;
    private static readonly int[] StageTargetRps = { 100, 500, 1000, 2000 };

    private static bool StressEnabled() =>
        string.Equals(
            Environment.GetEnvironmentVariable("StressTest__Enabled"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    private sealed record StageResult(
        int TargetRps,
        long Dispatched,
        long Failed,
        long Exceptions,
        double ElapsedSeconds,
        double ActualRps,
        long P50Us,
        long P95Us,
        long P99Us,
        long MaxUs,
        long WsStart,
        long WsEnd,
        double CpuMs,
        int Gc0,
        int Gc1,
        int Gc2);

    [Fact]
    public async Task Stress_Ramp_100_500_1k_2k_Saturation_Curve()
    {
        if (!StressEnabled()) return;

        var host = TestHost.ForTodo();
        var process = Process.GetCurrentProcess();
        var results = new List<StageResult>(StageTargetRps.Length);

        foreach (var targetRps in StageTargetRps)
        {
            var stage = await RunStage(host, process, targetRps);
            results.Add(stage);

            Console.WriteLine(
                $"[§5.3.3 stress stage rps={stage.TargetRps}] " +
                $"dispatched={stage.Dispatched} failed={stage.Failed} exceptions={stage.Exceptions} " +
                $"actualRps={stage.ActualRps:F1} elapsed={stage.ElapsedSeconds:F2}s " +
                $"latencyUs(p50/p95/p99/max)={stage.P50Us}/{stage.P95Us}/{stage.P99Us}/{stage.MaxUs} " +
                $"wsStart={stage.WsStart / 1024 / 1024}MB wsEnd={stage.WsEnd / 1024 / 1024}MB " +
                $"cpuMs={stage.CpuMs:F0} gc(0/1/2)={stage.Gc0}/{stage.Gc1}/{stage.Gc2}");
        }

        // ── Saturation analysis (post-run summary line) ──
        var baselineP99 = Math.Max(1, results[0].P99Us);
        Console.WriteLine($"[§5.3.3 stress summary] baselineP99Us={baselineP99}");
        for (var i = 0; i < results.Count; i++)
        {
            var r = results[i];
            var errorRate = r.Dispatched == 0
                ? 0.0
                : (double)(r.Failed + r.Exceptions) / r.Dispatched;
            var throughputRatio = r.ActualRps / Math.Max(1.0, r.TargetRps);
            var p99Ratio = (double)r.P99Us / baselineP99;

            var flags = new List<string>();
            if (errorRate > 0.01) flags.Add("ERROR>1%");
            if (p99Ratio > 10.0) flags.Add("P99>10×BASE");
            if (throughputRatio < 0.5) flags.Add("THROUGHPUT_COLLAPSE");

            Console.WriteLine(
                $"[§5.3.3 stress summary stage={i} rps={r.TargetRps}] " +
                $"throughputRatio={throughputRatio:F2} " +
                $"p99Ratio={p99Ratio:F2} " +
                $"errorRate={errorRate:P2} " +
                $"flags={(flags.Count == 0 ? "none" : string.Join(",", flags))}");
        }

        // ── Acceptance criteria (ST1–ST5) ──

        // ST4 — no undefined / system crash. The harness recorded
        // exceptions per stage but the test process must not have
        // collapsed; reaching this assertion is itself the proof.
        // We additionally assert that no stage hit a runaway exception
        // explosion (>50% of dispatches throwing).
        foreach (var r in results)
        {
            var explosionRate = r.Dispatched == 0
                ? 0.0
                : (double)r.Exceptions / r.Dispatched;
            Assert.True(explosionRate < 0.5,
                $"ST4 stage rps={r.TargetRps} exception rate ran away: " +
                $"exceptions={r.Exceptions} dispatched={r.Dispatched}");
        }

        // ST1 / ST2 — saturation point and degradation curve are
        // load-bearing on per-stage data being captured for every
        // stage. If any stage produced zero dispatches the curve is
        // unobservable.
        foreach (var r in results)
        {
            Assert.True(r.Dispatched > 0,
                $"ST1/ST2 stage rps={r.TargetRps} produced zero dispatches");
        }

        // ST3 — first failure mode is the first stage where any
        // critical-detection-point flag fires. We don't assert one
        // exists (a fully-stable run is also valid evidence — it just
        // means saturation was not reached inside the 2k cap).
    }

    private static async Task<StageResult> RunStage(
        TestHost host, Process process, int targetRps)
    {
        var dispatched = 0L;
        var failed = 0L;
        var exceptions = 0L;
        var latenciesUs = new ConcurrentQueue<long>();

        var wsStart = Environment.WorkingSet;
        var cpuStart = process.TotalProcessorTime.TotalMilliseconds;
        var gc0Start = GC.CollectionCount(0);
        var gc1Start = GC.CollectionCount(1);
        var gc2Start = GC.CollectionCount(2);

        using var stopCts = new CancellationTokenSource(TimeSpan.FromSeconds(StageSeconds));
        var stopwatch = Stopwatch.StartNew();

        // Per-worker pacing: each worker drives its share of the
        // stage's target RPS via deadline-locked sleeps. Aggregate
        // rate across N workers converges on the target.
        var perWorkerRps = (double)targetRps / WorkerCount;
        var workers = Enumerable.Range(0, WorkerCount)
            .Select(_ => Task.Run(async () =>
            {
                var workerStopwatch = Stopwatch.StartNew();
                var workerDispatched = 0;
                while (!stopCts.IsCancellationRequested)
                {
                    var aggregateId = Guid.NewGuid();
                    var ctx = host.NewTodoContext(aggregateId);

                    var rqStart = Stopwatch.GetTimestamp();
                    try
                    {
                        var result = await host.ControlPlane.ExecuteAsync(
                            new CreateTodoCommand(aggregateId, $"stress-{aggregateId}"),
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
                    latenciesUs.Enqueue(elapsedUs);

                    Interlocked.Increment(ref dispatched);
                    workerDispatched++;

                    // Deadline-locked pacing per worker.
                    var expectedMs = workerDispatched * 1000.0 / perWorkerRps;
                    var actualMs = workerStopwatch.Elapsed.TotalMilliseconds;
                    if (actualMs < expectedMs)
                    {
                        var sleepMs = (int)(expectedMs - actualMs);
                        if (sleepMs > 0)
                        {
                            try
                            {
                                await Task.Delay(sleepMs, stopCts.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                return;
                            }
                        }
                    }
                }
            }))
            .ToArray();

        await Task.WhenAll(workers);
        stopwatch.Stop();

        var wsEnd = Environment.WorkingSet;
        var cpuEnd = process.TotalProcessorTime.TotalMilliseconds;
        var gc0End = GC.CollectionCount(0);
        var gc1End = GC.CollectionCount(1);
        var gc2End = GC.CollectionCount(2);

        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
        var actualRps = dispatched / Math.Max(1.0, elapsedSeconds);

        var sortedUs = latenciesUs.ToArray();
        Array.Sort(sortedUs);

        return new StageResult(
            TargetRps: targetRps,
            Dispatched: dispatched,
            Failed: failed,
            Exceptions: exceptions,
            ElapsedSeconds: elapsedSeconds,
            ActualRps: actualRps,
            P50Us: Percentile(sortedUs, 0.50),
            P95Us: Percentile(sortedUs, 0.95),
            P99Us: Percentile(sortedUs, 0.99),
            MaxUs: sortedUs.Length > 0 ? sortedUs[^1] : 0,
            WsStart: wsStart,
            WsEnd: wsEnd,
            CpuMs: cpuEnd - cpuStart,
            Gc0: gc0End - gc0Start,
            Gc1: gc1End - gc1Start,
            Gc2: gc2End - gc2Start);
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
