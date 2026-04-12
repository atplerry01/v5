using Whyce.Platform.Host.Adapters;
using Whyce.Projections.Orchestration.Workflow;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Orchestration.Workflow;
using Whyce.Shared.Contracts.Projections.Orchestration.Workflow;

namespace Whycespace.Tests.Integration.Projections;

/// <summary>
/// phase1-gate-projection-hardening: covers replay safety (E2), in-place
/// mutation removal (#14), and last_event_id idempotency for the workflow
/// projection. Pure in-memory tests against the placeholder store and the
/// real handler — no infrastructure required.
/// </summary>
public sealed class WorkflowProjectionHardeningTest
{
    private static readonly Guid AggA = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static (WorkflowExecutionProjectionHandler handler, InMemoryWorkflowExecutionProjectionStore store) NewHandler()
    {
        var store = new InMemoryWorkflowExecutionProjectionStore();
        var handler = new WorkflowExecutionProjectionHandler(store);
        return (handler, store);
    }

    private static FakeEnvelope Envelope(Guid eventId, object payload, Guid? aggregateId = null) => new()
    {
        EventId = eventId,
        AggregateId = aggregateId ?? AggA,
        Payload = payload,
        EventType = payload.GetType().Name,
    };

    // ─────────────────────────────────────────────────────────────────
    // E2 — replay safety: log-and-skip on missing prior state
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StepCompleted_for_unknown_workflow_is_a_noop_not_a_throw()
    {
        var (handler, store) = NewHandler();

        // Step-completed event arrives before any started-event was projected.
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowStepCompletedEventSchema(AggA, 0, "step-0", "hash", "output")));

        Assert.Null(await store.GetAsync(AggA));
    }

    [Fact]
    public async Task Completed_for_unknown_workflow_is_a_noop_not_a_throw()
    {
        var (handler, store) = NewHandler();

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionCompletedEventSchema(AggA, "hash")));

        Assert.Null(await store.GetAsync(AggA));
    }

    [Fact]
    public async Task Failed_for_unknown_workflow_is_a_noop_not_a_throw()
    {
        var (handler, store) = NewHandler();

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionFailedEventSchema(AggA, "step-x", "boom")));

        Assert.Null(await store.GetAsync(AggA));
    }

    // ─────────────────────────────────────────────────────────────────
    // #14 — mutation isolation
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StepOutputs_returned_from_store_cannot_corrupt_stored_state()
    {
        var (handler, store) = NewHandler();

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionStartedEventSchema(AggA, "wf", null)));
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowStepCompletedEventSchema(AggA, 0, "step-0", "h0", "out0")));

        var first = await store.GetAsync(AggA);
        Assert.NotNull(first);
        Assert.Single(first!.StepOutputs);
        Assert.Equal("out0", first.StepOutputs["step-0"]);

        // Attempt to mutate via the returned reference. The contract is
        // IReadOnlyDictionary so the cast is what a misbehaving caller would
        // do; the defensive copy in the store must absorb it.
        if (first.StepOutputs is IDictionary<string, object?> mutable)
        {
            try { mutable["evil"] = "mutation"; } catch { /* ignored */ }
        }

        // Re-read: the store must NOT have been corrupted by the cast above.
        var second = await store.GetAsync(AggA);
        Assert.NotNull(second);
        Assert.Single(second!.StepOutputs);
        Assert.False(second.StepOutputs.ContainsKey("evil"));
    }

    [Fact]
    public async Task Multiple_step_completed_events_accumulate_outputs_correctly()
    {
        var (handler, store) = NewHandler();

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionStartedEventSchema(AggA, "wf", null)));
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowStepCompletedEventSchema(AggA, 0, "s0", "h0", "v0")));
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowStepCompletedEventSchema(AggA, 1, "s1", "h1", "v1")));
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowStepCompletedEventSchema(AggA, 2, "s2", "h2", "v2")));

        var model = await store.GetAsync(AggA);
        Assert.NotNull(model);
        Assert.Equal(3, model!.StepOutputs.Count);
        Assert.Equal("v0", model.StepOutputs["s0"]);
        Assert.Equal("v1", model.StepOutputs["s1"]);
        Assert.Equal("v2", model.StepOutputs["s2"]);
        Assert.Equal(2, model.CurrentStepIndex);
        Assert.Equal("h2", model.ExecutionHash);
    }

    // ─────────────────────────────────────────────────────────────────
    // Idempotency — same event id is a no-op
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Replay_of_same_event_id_is_a_noop_for_started()
    {
        var (handler, store) = NewHandler();
        var eid = Guid.NewGuid();

        await handler.HandleAsync(Envelope(eid,
            new WorkflowExecutionStartedEventSchema(AggA, "wf", null)));
        // A second projection of the SAME envelope must not change anything.
        await handler.HandleAsync(Envelope(eid,
            new WorkflowExecutionStartedEventSchema(AggA, "DIFFERENT WORKFLOW NAME", null)));

        var model = await store.GetAsync(AggA);
        Assert.NotNull(model);
        Assert.Equal("wf", model!.WorkflowName);
        Assert.Equal(eid, model.LastEventId);
    }

    [Fact]
    public async Task Replay_of_same_step_completed_does_not_double_apply()
    {
        var (handler, store) = NewHandler();
        var stepEventId = Guid.NewGuid();

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionStartedEventSchema(AggA, "wf", null)));
        await handler.HandleAsync(Envelope(stepEventId,
            new WorkflowStepCompletedEventSchema(AggA, 0, "s0", "h0", "v0")));
        // Replay
        await handler.HandleAsync(Envelope(stepEventId,
            new WorkflowStepCompletedEventSchema(AggA, 99, "s0", "DIFFERENT_HASH", "v0")));

        var model = await store.GetAsync(AggA);
        Assert.NotNull(model);
        Assert.Equal(0, model!.CurrentStepIndex);
        Assert.Equal("h0", model.ExecutionHash);
        Assert.Single(model.StepOutputs);
    }

    [Fact]
    public async Task Distinct_event_ids_for_same_aggregate_all_apply()
    {
        var (handler, store) = NewHandler();

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionStartedEventSchema(AggA, "wf", null)));
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowStepCompletedEventSchema(AggA, 0, "s0", "h0", "v0")));
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionCompletedEventSchema(AggA, "final-hash")));

        var model = await store.GetAsync(AggA);
        Assert.NotNull(model);
        Assert.Equal("Completed", model!.Status);
        Assert.Equal("final-hash", model.ExecutionHash);
    }

    // ─────────────────────────────────────────────────────────────────
    // Store contract — defensive copies on both read and write
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task InMemoryStore_GetAsync_returns_a_defensive_copy()
    {
        var store = new InMemoryWorkflowExecutionProjectionStore();
        var seed = new WorkflowExecutionReadModel
        {
            WorkflowExecutionId = AggA,
            WorkflowName = "wf",
            StepOutputs = new Dictionary<string, object?> { ["s0"] = "v0" },
        };
        await store.UpsertAsync(seed);

        var first = await store.GetAsync(AggA);
        var second = await store.GetAsync(AggA);

        Assert.NotNull(first);
        Assert.NotNull(second);
        // Two GetAsync calls must return distinct StepOutputs instances.
        Assert.NotSame(first!.StepOutputs, second!.StepOutputs);
        // The returned dictionaries must also not be the seed instance.
        Assert.NotSame(seed.StepOutputs, first.StepOutputs);
    }

    // ─────────────────────────────────────────────────────────────────
    // Test envelope
    // ─────────────────────────────────────────────────────────────────

    private sealed class FakeEnvelope : IEventEnvelope
    {
        public Guid EventId { get; init; }
        public Guid AggregateId { get; init; }
        public Guid CorrelationId { get; init; } = Guid.Empty;
        public string EventType { get; init; } = string.Empty;
        public string EventName { get; init; } = string.Empty;
        public string EventVersion { get; init; } = string.Empty;
        public string SchemaHash { get; init; } = string.Empty;
        public object Payload { get; init; } = new();
        public string ExecutionHash { get; init; } = string.Empty;
        public string PolicyHash { get; init; } = string.Empty;
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UnixEpoch;
        public int SequenceNumber { get; init; }
        public string Classification { get; init; } = string.Empty;
        public string Context { get; init; } = string.Empty;
        public string Domain { get; init; } = string.Empty;
    }
}
