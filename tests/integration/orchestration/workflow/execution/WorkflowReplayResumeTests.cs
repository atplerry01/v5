using Whyce.Engines.T1M.Lifecycle;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Integration.Orchestration.Workflow.Execution;

/// <summary>
/// H8 replay/resume closure coverage. Drives the real
/// <see cref="WorkflowExecutionReplayService"/> through the full canonical
/// flow (Start → Step → Fail → Resume → Step → Complete) and verifies the
/// invariants stated in the WBSM v3.5 H8 prompt:
///
/// - Resume only valid from Failed
/// - WorkflowExecutionResumedEvent emitted exactly once on resume
/// - Replay-after-resume reconstructs identical state
/// - Double replay produces identical state
/// - Resume twice without intervening failure throws
/// </summary>
public sealed class WorkflowReplayResumeTests
{
    private static readonly Guid ExecutionId = Guid.Parse("00000000-0000-0000-0000-000000000020");

    [Fact]
    public async Task Fail_Then_Resume_Then_Complete_Works()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));

        // Phase 1: original run that fails on step 2.
        var live = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        live.CompleteStep(0, "Validate", "h0");
        live.CompleteStep(1, "Reserve", "h1");
        live.Fail("Charge", "card declined");
        store.Append(live.DomainEvents);

        // Phase 2: dispatcher-style resume — read state, gate on Failed, ask
        // the replay service for the WorkflowExecutionResumedEvent, append
        // it, then continue the engine simulation.
        var state = await service.ReplayAsync(ExecutionId);
        Assert.NotNull(state);
        Assert.Equal("Failed", state!.Status);
        Assert.Equal(2, state.NextStepIndex);

        var resumed = await service.ResumeAsync(ExecutionId);
        Assert.IsType<WorkflowExecutionResumedEvent>(resumed);

        // Engine emits the remaining steps + Complete on top of the persisted
        // stream. We simulate that here by raising them on a fresh aggregate
        // instance loaded from the previous history + the resume event.
        var continuation = NewBareAggregate();
        var historySoFar = store.Load(ExecutionId).Concat(new[] { resumed }).ToList();
        continuation.LoadFromHistory(historySoFar);
        Assert.Equal(WorkflowExecutionStatus.Running, continuation.Status);

        continuation.CompleteStep(2, "Charge", "h2");
        continuation.CompleteStep(3, "Ship", "h3");
        continuation.Complete("h-final");

        store.Append(new[] { resumed }.Concat(continuation.DomainEvents).ToList());

        // Final replay reflects the entire lifecycle.
        var final = await service.ReplayAsync(ExecutionId);
        Assert.NotNull(final);
        Assert.Equal("Completed", final!.Status);
        Assert.Equal(4, final.NextStepIndex);
    }

    [Fact]
    public async Task Resume_Without_Failure_Throws()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));

        var live = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        live.CompleteStep(0, "Validate", "h0");
        store.Append(live.DomainEvents);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResumeAsync(ExecutionId));
    }

    [Fact]
    public async Task Resume_When_No_Events_Throws()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResumeAsync(ExecutionId));
    }

    [Fact]
    public async Task Replay_After_Resume_Matches_Expected_State()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));

        var live = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        live.CompleteStep(0, "Validate", "h0");
        live.Fail("Reserve", "out of stock");
        store.Append(live.DomainEvents);

        var resumed = await service.ResumeAsync(ExecutionId);
        store.Append(new[] { resumed });

        var state = await service.ReplayAsync(ExecutionId);
        Assert.NotNull(state);
        Assert.Equal("Running", state!.Status);
        Assert.Equal(1, state.NextStepIndex);
        Assert.Equal("h0", state.ExecutionHash);
    }

    [Fact]
    public async Task Double_Replay_Produces_Identical_State()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));

        var live = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        live.CompleteStep(0, "Validate", "h0");
        live.Fail("Reserve", "boom");
        store.Append(live.DomainEvents);

        var first = await service.ReplayAsync(ExecutionId);
        var second = await service.ReplayAsync(ExecutionId);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first!.Status, second!.Status);
        Assert.Equal(first.NextStepIndex, second.NextStepIndex);
        Assert.Equal(first.ExecutionHash, second.ExecutionHash);
        Assert.Equal(first.WorkflowName, second.WorkflowName);
    }

    [Fact]
    public async Task Resume_Emits_ResumedEvent_Exactly_Once()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));

        var live = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        live.Fail("Validate", "boom");
        store.Append(live.DomainEvents);

        var resumed = await service.ResumeAsync(ExecutionId);
        store.Append(new[] { resumed });

        var stream = store.Load(ExecutionId);
        var resumedCount = stream.Count(e => e is WorkflowExecutionResumedEvent);
        Assert.Equal(1, resumedCount);
    }

    [Fact]
    public async Task Resume_Twice_Without_Intervening_Failure_Throws()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));

        var live = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        live.Fail("Validate", "boom");
        store.Append(live.DomainEvents);

        var resumed = await service.ResumeAsync(ExecutionId);
        store.Append(new[] { resumed });

        // Aggregate is now Running again. A second resume MUST be rejected.
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResumeAsync(ExecutionId));
    }

    private static WorkflowExecutionAggregate NewBareAggregate() =>
        (WorkflowExecutionAggregate)System.Activator.CreateInstance(
            typeof(WorkflowExecutionAggregate), nonPublic: true)!;

    private sealed class MutableEventStore : IEventStore
    {
        private readonly List<object> _events = [];

        // phase1.5-S5.2.5 / TB-1: aligned with the post-TC-5 IEventStore
        // contract that carries CancellationToken end-to-end.
        public Task<IReadOnlyList<object>> LoadEventsAsync(
            Guid aggregateId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<object>>(_events.ToList());

        public Task AppendEventsAsync(
            Guid aggregateId,
            IReadOnlyList<object> events,
            int expectedVersion,
            CancellationToken cancellationToken = default)
        {
            _events.AddRange(events);
            return Task.CompletedTask;
        }

        public void Append(IEnumerable<object> events) => _events.AddRange(events);
        public IReadOnlyList<object> Load(Guid id) => _events.ToList();
    }
}
