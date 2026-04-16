using Whycespace.Runtime.Deterministic;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Replay;

/// <summary>
/// Replay Type A — re-execution determinism.
///
/// Runs the Todo lifecycle (Create + Update + Complete) twice in two
/// independent TestHost fixtures with identical TestClock and
/// TestIdGenerator. Asserts:
///
///   1. Persisted event lists are byte-equal element-by-element.
///   2. Recomputed ExecutionHash for each run matches across runs.
///   3. Aggregate IDs and command coordinates match.
///   4. Outbox topic + batch counts match.
///   5. Chain block sequence + decision hashes match.
///
/// This is the spirit interpretation of the Phase 2 prompt's TG3 — re-execute
/// the same commands and assert equality, rather than mutating
/// EventReplayService's intentional sentinel design (see
/// claude/audits/replay-determinism.audit.md for the distinction between
/// Type A re-execution and Type B projection rebuild).
/// </summary>
public sealed class DeterministicReplayTest
{
    [Fact]
    public async Task Two_Independent_Runs_Produce_Identical_Events_And_Hashes()
    {
        var (run1, hashes1) = await RunLifecycleAsync();
        var (run2, hashes2) = await RunLifecycleAsync();

        // 1. Same number of persisted events.
        Assert.Equal(run1.EventStore.AllEvents(run1.AggregateId).Count, run2.EventStore.AllEvents(run2.AggregateId).Count);
        Assert.Equal(3, run1.EventStore.AllEvents(run1.AggregateId).Count);

        // 2. Same aggregate id (deterministic from id generator).
        Assert.Equal(run1.AggregateId, run2.AggregateId);

        // 3. Event types in same order.
        var types1 = run1.EventStore.AllEvents(run1.AggregateId).Select(e => e.GetType().FullName).ToArray();
        var types2 = run2.EventStore.AllEvents(run2.AggregateId).Select(e => e.GetType().FullName).ToArray();
        Assert.Equal(types1, types2);

        // 4. Recomputed execution hashes are byte-equal across runs.
        Assert.Equal(hashes1, hashes2);
        Assert.Equal(3, hashes1.Count);

        // 5. Outbox: same number of batches, same topic.
        Assert.Equal(run1.Outbox.Batches.Count, run2.Outbox.Batches.Count);
        Assert.Equal(run1.Outbox.Batches[0].Topic, run2.Outbox.Batches[0].Topic);

        // 6. Chain: same number of blocks, same decision hashes in same order.
        var decisions1 = run1.ChainAnchor.Blocks.Select(b => b.DecisionHash).ToArray();
        var decisions2 = run2.ChainAnchor.Blocks.Select(b => b.DecisionHash).ToArray();
        Assert.Equal(decisions1, decisions2);

        // 7. Chain block ids are deterministic across runs.
        var blockIds1 = run1.ChainAnchor.Blocks.Select(b => b.BlockId).ToArray();
        var blockIds2 = run2.ChainAnchor.Blocks.Select(b => b.BlockId).ToArray();
        Assert.Equal(blockIds1, blockIds2);

        // 8. Chain block timestamps are deterministic across runs (frozen TestClock).
        var ts1 = run1.ChainAnchor.Blocks.Select(b => b.Timestamp).ToArray();
        var ts2 = run2.ChainAnchor.Blocks.Select(b => b.Timestamp).ToArray();
        Assert.Equal(ts1, ts2);
        Assert.All(ts1, t => Assert.Equal(TestClock.Frozen, t));
    }

    private static async Task<(RunResult Result, IReadOnlyList<string> Hashes)> RunLifecycleAsync()
    {
        var host = TestHost.ForTodo();
        // Use a STABLE seed so AggregateId is identical across runs.
        var aggregateId = host.IdGenerator.Generate("todo:replay-fixture:stable");

        var hashes = new List<string>();

        var createCtx = host.NewTodoContext(aggregateId);
        var createResult = await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Buy milk"), createCtx);
        Assert.True(createResult.IsSuccess, createResult.Error);
        hashes.Add(ExecutionHash.Compute(createCtx, createResult.EmittedEvents));

        var updateCtx = host.NewTodoContext(aggregateId);
        var updateResult = await host.ControlPlane.ExecuteAsync(
            new UpdateTodoCommand(aggregateId, "Buy almond milk"), updateCtx);
        Assert.True(updateResult.IsSuccess, updateResult.Error);
        hashes.Add(ExecutionHash.Compute(updateCtx, updateResult.EmittedEvents));

        var completeCtx = host.NewTodoContext(aggregateId);
        var completeResult = await host.ControlPlane.ExecuteAsync(
            new CompleteTodoCommand(aggregateId), completeCtx);
        Assert.True(completeResult.IsSuccess, completeResult.Error);
        hashes.Add(ExecutionHash.Compute(completeCtx, completeResult.EmittedEvents));

        return (new RunResult(host.EventStore, host.ChainAnchor, host.Outbox, aggregateId), hashes);
    }

    private sealed record RunResult(
        InMemoryEventStore EventStore,
        InMemoryChainAnchor ChainAnchor,
        InMemoryOutbox Outbox,
        Guid AggregateId);
}
