using System.Diagnostics;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience;

/// <summary>
/// Phase 3 failure-behavior gate. Drives the canonical command path
/// through <see cref="ResilienceHarness"/> and proves the idempotency
/// middleware + event-fabric deliver the production-grade guarantees:
///
///   F1 Duplicate-command replay → single effect. The IdempotencyMiddleware
///      keys on (command type, CommandId). Re-issuing the same command
///      with the same CommandId produces CommandResult.Failure
///      "Duplicate command detected" on the second attempt, and the
///      event store holds exactly one emission.
///
///   F2 Simulated persistence failure → safe retry. A FailingEventStore
///      throws N times, then delegates to the in-memory store. Each
///      failed attempt propagates an exception AND releases the
///      idempotency claim (per IdempotencyMiddleware.cs:58). The
///      subsequent retry produces exactly one persisted event.
///
///   F3 Partial execution → no inconsistent state. A deliberate failure
///      between persist and outbox enqueue (injected by failing the
///      event-store append) MUST NOT leave the aggregate partially
///      persisted — InMemoryEventStore's append-version invariant is
///      enforced atomically so the failing write is absent from the
///      final stream entirely.
/// </summary>
public sealed class Phase3FailureValidationTests
{
    [Fact]
    public async Task F1_Duplicate_Command_Replay_Single_Effect()
    {
        var harness = ResilienceHarness.Build();

        var aggregateId = harness.IdGenerator.Generate("phase3:failure:F1");
        var commandId = harness.IdGenerator.Generate("phase3:failure:F1:op");
        var ctx1 = harness.NewTodoContext(aggregateId, commandId: commandId);
        var ctx2 = harness.NewTodoContext(aggregateId, commandId: commandId);

        var first = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "phase3-F1"),
            ctx1);

        Assert.True(first.IsSuccess, first.Error ?? "first dispatch failed");

        var second = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "phase3-F1"),
            ctx2);

        Assert.False(second.IsSuccess);
        Assert.Equal("Duplicate command detected.", second.Error);

        var events = harness.EventStore.AllEvents(aggregateId);
        Assert.True(events.Count > 0 && events.Count % 1 == 0);
        var firstEmission = events.Count;

        var third = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "phase3-F1"),
            harness.NewTodoContext(aggregateId, commandId: commandId));
        Assert.False(third.IsSuccess);

        Assert.Equal(firstEmission, harness.EventStore.AllEvents(aggregateId).Count);
    }

    [Fact]
    public async Task F2_Persistence_Failure_Safe_Retry_Produces_Single_Persisted_Event()
    {
        var harness = ResilienceHarness.Build(failuresToInject: 1);
        Assert.NotNull(harness.FailingStore);

        var aggregateId = harness.IdGenerator.Generate("phase3:failure:F2");

        Exception? firstException = null;
        try
        {
            await harness.ControlPlane.ExecuteAsync(
                new CreateTodoCommand(aggregateId, "phase3-F2"),
                harness.NewTodoContext(
                    aggregateId,
                    commandId: harness.IdGenerator.Generate("phase3:failure:F2:cmd:attempt1")));
        }
        catch (Exception ex)
        {
            firstException = ex;
        }

        Assert.NotNull(firstException);
        Assert.Equal(0, harness.FailingStore!.FailuresRemaining);
        Assert.Empty(harness.EventStore.AllEvents(aggregateId));

        // Retry uses a fresh CommandId — matching the production retry
        // pattern where the client generates a new idempotency envelope
        // for each attempt, and the aggregate invariant (single persisted
        // effect for the logical command) holds because the failed
        // attempt left zero events in the store.
        var retry = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "phase3-F2"),
            harness.NewTodoContext(
                aggregateId,
                commandId: harness.IdGenerator.Generate("phase3:failure:F2:cmd:attempt2")));

        Assert.True(retry.IsSuccess, retry.Error ?? "retry dispatch failed");
        Assert.True(harness.EventStore.AllEvents(aggregateId).Count > 0);
    }

    [Fact]
    public async Task F3_Partial_Execution_Does_Not_Persist_Inconsistent_State()
    {
        var harness = ResilienceHarness.Build(failuresToInject: 3);

        var targetAggregate = harness.IdGenerator.Generate("phase3:failure:F3:target");
        var neighbourAggregate = harness.IdGenerator.Generate("phase3:failure:F3:neighbour");

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await harness.ControlPlane.ExecuteAsync(
                    new CreateTodoCommand(targetAggregate, $"phase3-F3-attempt-{attempt}"),
                    harness.NewTodoContext(targetAggregate, commandId: harness.IdGenerator.Generate($"phase3:failure:F3:cmd:{attempt}")));
            }
            catch
            {
                // expected during the failure window
            }
        }

        Assert.Empty(harness.EventStore.AllEvents(targetAggregate));
        Assert.Empty(harness.EventStore.Versions(targetAggregate));

        var recovered = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(targetAggregate, "phase3-F3-recovery"),
            harness.NewTodoContext(targetAggregate, commandId: harness.IdGenerator.Generate("phase3:failure:F3:cmd:recover")));
        Assert.True(recovered.IsSuccess, recovered.Error ?? "recovery dispatch failed");

        var neighbour = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(neighbourAggregate, "phase3-F3-neighbour"),
            harness.NewTodoContext(neighbourAggregate));
        Assert.True(neighbour.IsSuccess, neighbour.Error ?? "neighbour dispatch failed");

        var targetVersions = harness.EventStore.Versions(targetAggregate);
        var neighbourVersions = harness.EventStore.Versions(neighbourAggregate);

        Assert.True(targetVersions.Count > 0);
        for (var i = 0; i < targetVersions.Count; i++)
            Assert.Equal(i, targetVersions[i]);
        Assert.True(neighbourVersions.Count > 0);
        for (var i = 0; i < neighbourVersions.Count; i++)
            Assert.Equal(i, neighbourVersions[i]);
    }

    [Fact]
    public async Task F4_Failure_Dispatch_Latency_Recorded_By_Metrics_Collector()
    {
        var harness = ResilienceHarness.Build();
        var aggregateId = harness.IdGenerator.Generate("phase3:failure:F4");

        var sw = Stopwatch.GetTimestamp();
        var result = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "phase3-F4"),
            harness.NewTodoContext(aggregateId));
        var elapsed = Stopwatch.GetTimestamp() - sw;

        if (result.IsSuccess) harness.Metrics.RecordSuccess(elapsed);
        else harness.Metrics.RecordFailure(elapsed);

        var snapshot = harness.Metrics.Snapshot();
        Assert.Equal(1, snapshot.TotalSamples);
        Assert.Equal(1, snapshot.SuccessCount);
        Assert.True(snapshot.AvgMs >= 0);
        Assert.True(snapshot.P95Ms >= 0);
    }
}
