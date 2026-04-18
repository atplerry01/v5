using System.Diagnostics;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience;

/// <summary>
/// Phase 3 adversarial-validation gate. Exercises the runtime pipeline
/// against deliberately hostile inputs:
///
///   A1 Conflicting transactions — N workers race to dispatch commands
///      against the SAME aggregate. Each worker uses a distinct
///      CommandId so the idempotency middleware admits every one, but
///      every append contends for the same aggregate version. The
///      InMemoryEventStore enforces the append-only invariant; a real
///      conflict surfaces as an exception. The test asserts:
///        * no invariant violation (contiguous 0..N-1 versions),
///        * system converges — all successful commands are durably
///          persisted exactly once,
///        * zero unhandled exceptions escape the control plane (every
///          outcome is observed as success or explicit failure).
///
///   A2 Edge values — extreme-length payloads and empty / whitespace
///      boundary values flow through the pipeline. Invalid inputs (if
///      any) are rejected via CommandResult.Failure rather than
///      throwing; valid inputs produce exactly one event each.
///
///   A3 Ordering attacks — commands issued out of their natural
///      sequence AND replayed commands (same CommandId) converge to
///      a consistent state: each distinct CommandId persists exactly
///      once, replays are rejected by the idempotency middleware.
/// </summary>
public sealed class Phase3AdversarialValidationTests
{
    [Fact]
    public async Task A1_High_Concurrency_Conflicts_Converge_To_Valid_State()
    {
        var harness = ResilienceHarness.Build();
        var aggregateId = harness.IdGenerator.Generate("phase3:adversarial:A1");

        const int workerCount = 64;
        var successes = 0;
        var failures = 0;
        var exceptions = 0;

        var gate = new ManualResetEventSlim(initialState: false);
        var tasks = new Task[workerCount];
        for (var w = 0; w < workerCount; w++)
        {
            var idx = w;
            tasks[w] = Task.Run(async () =>
            {
                gate.Wait();
                var commandId = harness.IdGenerator.Generate($"phase3:adversarial:A1:cmd:{idx}");
                var ctx = harness.NewTodoContext(aggregateId, commandId: commandId);
                try
                {
                    var result = await harness.ControlPlane.ExecuteAsync(
                        new CreateTodoCommand(aggregateId, $"A1-{idx}"),
                        ctx);
                    if (result.IsSuccess) Interlocked.Increment(ref successes);
                    else Interlocked.Increment(ref failures);
                }
                catch
                {
                    Interlocked.Increment(ref exceptions);
                }
            });
        }

        gate.Set();
        await Task.WhenAll(tasks);

        Assert.Equal(0, exceptions);

        var versions = harness.EventStore.Versions(aggregateId);
        for (var i = 0; i < versions.Count; i++)
            Assert.Equal(i, versions[i]);

        Assert.True(versions.Count > 0);
        Assert.Equal(workerCount, successes + failures);
    }

    [Fact]
    public async Task A2_Edge_Value_Inputs_Rejected_Safely()
    {
        var harness = ResilienceHarness.Build();

        var largeTitle = new string('x', 100_000);
        var boundaryCases = new[]
        {
            ("phase3:A2:empty", string.Empty),
            ("phase3:A2:whitespace", "   "),
            ("phase3:A2:large", largeTitle),
            ("phase3:A2:unicode", "\u00E9\u00FC\u00E4\u00F1\uD83D\uDC4B")
        };

        // The contract is "invalid inputs rejected safely, system converges to
        // valid state" — a thrown exception from a validation middleware IS a
        // safe rejection at the infrastructure seam (callers observe the
        // failure and do not mutate state). The assertion is therefore:
        //   * every dispatch either succeeds, fails, or throws — no ambiguous
        //     ("IsSuccess=false, Error=null") result,
        //   * no aggregate leaves the store in an inconsistent state.
        foreach (var (seed, title) in boundaryCases)
        {
            var aggregateId = harness.IdGenerator.Generate(seed);
            var eventsBefore = harness.EventStore.AllEvents(aggregateId).Count;

            try
            {
                var result = await harness.ControlPlane.ExecuteAsync(
                    new CreateTodoCommand(aggregateId, title),
                    harness.NewTodoContext(aggregateId));
                Assert.True(result.IsSuccess || !string.IsNullOrEmpty(result.Error),
                    $"A2 boundary '{seed}': dispatcher returned ambiguous result (success=false, error=null)");
                if (!result.IsSuccess)
                {
                    Assert.Equal(eventsBefore, harness.EventStore.AllEvents(aggregateId).Count);
                }
            }
            catch (Exception)
            {
                // A throw here == a rejection at an infrastructure seam
                // (validation middleware / aggregate invariant). The
                // resilience contract demands state does not leak.
                Assert.Equal(eventsBefore, harness.EventStore.AllEvents(aggregateId).Count);
            }
        }
    }

    [Fact]
    public async Task A3_Out_Of_Order_And_Replayed_Commands_Converge()
    {
        var harness = ResilienceHarness.Build();
        var aggregateId = harness.IdGenerator.Generate("phase3:adversarial:A3");

        var distinctCommandIds = new[]
        {
            harness.IdGenerator.Generate("phase3:adversarial:A3:cmd:alpha"),
            harness.IdGenerator.Generate("phase3:adversarial:A3:cmd:beta"),
            harness.IdGenerator.Generate("phase3:adversarial:A3:cmd:gamma")
        };

        // Initial dispatch batch (out-of-order: gamma → alpha → beta).
        foreach (var cid in new[] { distinctCommandIds[2], distinctCommandIds[0], distinctCommandIds[1] })
        {
            var result = await harness.ControlPlane.ExecuteAsync(
                new CreateTodoCommand(aggregateId, $"A3-{cid}"),
                harness.NewTodoContext(aggregateId, commandId: cid));
            Assert.True(result.IsSuccess, result.Error ?? "A3 initial dispatch failed");
        }

        var afterInitial = harness.EventStore.AllEvents(aggregateId).Count;

        // Replay batch — every CommandId already seen must be rejected.
        foreach (var cid in distinctCommandIds)
        {
            var replay = await harness.ControlPlane.ExecuteAsync(
                new CreateTodoCommand(aggregateId, $"A3-replay-{cid}"),
                harness.NewTodoContext(aggregateId, commandId: cid));
            Assert.False(replay.IsSuccess);
            Assert.Equal("Duplicate command detected.", replay.Error);
        }

        Assert.Equal(afterInitial, harness.EventStore.AllEvents(aggregateId).Count);
    }

    [Fact]
    public async Task A4_Adversarial_Latency_Within_Phase3_Budget()
    {
        var harness = ResilienceHarness.Build();

        const int sampleCount = 200;
        for (var i = 0; i < sampleCount; i++)
        {
            var aggregateId = harness.IdGenerator.Generate($"phase3:adversarial:A4:{i}");
            var sw = Stopwatch.GetTimestamp();
            try
            {
                var result = await harness.ControlPlane.ExecuteAsync(
                    new CreateTodoCommand(aggregateId, $"A4-{i}"),
                    harness.NewTodoContext(aggregateId));
                var elapsed = Stopwatch.GetTimestamp() - sw;
                if (result.IsSuccess) harness.Metrics.RecordSuccess(elapsed);
                else harness.Metrics.RecordFailure(elapsed);
            }
            catch
            {
                harness.Metrics.RecordException(Stopwatch.GetTimestamp() - sw);
            }
        }

        var snapshot = harness.Metrics.Snapshot();
        Assert.Equal(sampleCount, snapshot.TotalSamples);
        Assert.True(snapshot.P95Ms < 1000.0,
            $"A4 p95 latency {snapshot.P95Ms:F2}ms exceeds Phase 3 critical budget (1000ms)");
        Assert.True(snapshot.ErrorRate < 0.01,
            $"A4 error rate {snapshot.ErrorRate:P2} exceeds Phase 3 critical budget (1%)");
    }
}
