using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Orchestration.Workflow;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Orchestration.Workflow;
using Whycespace.Shared.Contracts.Projections.Orchestration.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Integration.Projections;

/// <summary>
/// R3.A.6 / R-WF-APPROVAL-PROJ-01 coverage. Asserts the workflow
/// projection reducers for Suspended / Cancelled / Resumed preserve
/// the canonical lifecycle <see cref="WorkflowExecutionReadModel.Status"/>
/// (<c>Suspended</c>, <c>Cancelled</c>, <c>Running</c>) and surface
/// derived approval semantics through
/// <see cref="WorkflowExecutionReadModel.ApprovalState"/> /
/// <see cref="WorkflowExecutionReadModel.ApprovalSignal"/> /
/// <see cref="WorkflowExecutionReadModel.ApprovalDecision"/> without
/// overloading canonical lifecycle values.
/// </summary>
public sealed class WorkflowApprovalProjectionTests
{
    private static readonly Guid Agg = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private const string HumanApproval = WorkflowApprovalErrors.HumanApprovalPrefix;
    private const string HumanApprovalGranted = WorkflowApprovalErrors.HumanApprovalGrantedPrefix;
    private const string HumanApprovalRejected = WorkflowApprovalErrors.HumanApprovalRejectedPrefix;

    private static (WorkflowExecutionProjectionHandler handler, InMemoryWorkflowExecutionProjectionStore store) NewHandler()
    {
        var store = new InMemoryWorkflowExecutionProjectionStore();
        var handler = new WorkflowExecutionProjectionHandler(store);
        return (handler, store);
    }

    private static FakeEnvelope Envelope(Guid eventId, object payload) => new()
    {
        EventId = eventId,
        AggregateId = Agg,
        Payload = payload,
        EventType = payload.GetType().Name,
    };

    private static async Task SeedStartedAsync(WorkflowExecutionProjectionHandler handler)
    {
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionStartedEventSchema(Agg, "wf")));
    }

    // ─────────────────────────────────────────────────────────────────
    // Suspended — canonical Status + derived ApprovalState
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Suspended_With_Human_Approval_Sets_AwaitingApproval_And_Parses_Signal()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionSuspendedEventSchema(Agg, "Approve", $"{HumanApproval}:risk-sign-off")));

        var model = await store.GetAsync(Agg);
        Assert.NotNull(model);
        Assert.Equal("Suspended", model!.Status);         // canonical lifecycle status preserved
        Assert.Equal("AwaitingApproval", model.ApprovalState);
        Assert.Equal("risk-sign-off", model.ApprovalSignal);
    }

    [Fact]
    public async Task Suspended_With_Bare_Human_Approval_Sets_AwaitingApproval_Null_Signal()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionSuspendedEventSchema(Agg, "Approve", HumanApproval)));

        var model = await store.GetAsync(Agg);
        Assert.NotNull(model);
        Assert.Equal("Suspended", model!.Status);
        Assert.Equal("AwaitingApproval", model.ApprovalState);
        Assert.Null(model.ApprovalSignal);
    }

    [Fact]
    public async Task Suspended_With_Non_Approval_Signal_Leaves_ApprovalState_Null()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionSuspendedEventSchema(Agg, "Wait", "timer_wait:30s")));

        var model = await store.GetAsync(Agg);
        Assert.NotNull(model);
        Assert.Equal("Suspended", model!.Status);         // canonical status still Suspended
        Assert.Null(model.ApprovalState);                 // but not an approval wait-state
    }

    // ─────────────────────────────────────────────────────────────────
    // Cancelled — canonical Status + derived ApprovalState
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancelled_With_Human_Approval_Rejected_Sets_Rejected_And_Parses_Decision()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionSuspendedEventSchema(Agg, "Approve", $"{HumanApproval}:risk-sign-off")));

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionCancelledEventSchema(Agg, "Approve",
                $"{HumanApprovalRejected}:risk-sign-off:actor/op-1:Risk-exceeded")));

        var model = await store.GetAsync(Agg);
        Assert.NotNull(model);
        Assert.Equal("Cancelled", model!.Status);         // canonical lifecycle status preserved
        Assert.Equal("Rejected", model.ApprovalState);
        Assert.Equal("Risk-exceeded", model.ApprovalDecision);
        Assert.Equal("risk-sign-off", model.ApprovalSignal);
    }

    [Fact]
    public async Task Cancelled_Without_Approval_Prefix_Leaves_ApprovalState_Null()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);

        // caller_cancellation carrier — the legacy R3.A.4 path
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionCancelledEventSchema(Agg, "Step", "caller_cancellation: OperationCanceledException: shutdown")));

        var model = await store.GetAsync(Agg);
        Assert.NotNull(model);
        Assert.Equal("Cancelled", model!.Status);
        Assert.Null(model.ApprovalState);                 // not an approval rejection
    }

    // ─────────────────────────────────────────────────────────────────
    // Resumed — approval-granted vs failure-retry
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Resumed_With_Approval_Granted_Sets_Granted_And_Clears_Decision()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionSuspendedEventSchema(Agg, "Approve", $"{HumanApproval}:risk-sign-off")));

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionResumedEventSchema(Agg, "Approve",
                $"{HumanApprovalGranted}:risk-sign-off:actor/op-1:CFO-approved")));

        var model = await store.GetAsync(Agg);
        Assert.NotNull(model);
        Assert.Equal("Running", model!.Status);
        Assert.Equal("Granted", model.ApprovalState);
        Assert.Equal("risk-sign-off", model.ApprovalSignal);
        Assert.Null(model.ApprovalDecision);              // cleared on grant
    }

    [Fact]
    public async Task Resumed_With_Failure_Retry_Leaves_ApprovalState_Unchanged()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionFailedEventSchema(Agg, "Charge", "card declined")));

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionResumedEventSchema(Agg, "Charge", "card declined")));

        var model = await store.GetAsync(Agg);
        Assert.NotNull(model);
        Assert.Equal("Running", model!.Status);
        Assert.Null(model.ApprovalState);                 // non-approval resume — no approval semantics
    }

    // ─────────────────────────────────────────────────────────────────
    // Canonical lifecycle status invariant (R-WF-APPROVAL-PROJ-01)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Canonical_Lifecycle_Status_Never_Overloaded_By_Approval_Semantics()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);

        // Suspend-for-approval
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionSuspendedEventSchema(Agg, "Approve", $"{HumanApproval}:risk-sign-off")));
        var afterSuspend = await store.GetAsync(Agg);
        Assert.Equal("Suspended", afterSuspend!.Status);   // NOT "AwaitingApproval"

        // Reject
        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionCancelledEventSchema(Agg, "Approve",
                $"{HumanApprovalRejected}:risk-sign-off:actor/op-1:no")));
        var afterReject = await store.GetAsync(Agg);
        Assert.Equal("Cancelled", afterReject!.Status);    // NOT "Rejected"
    }

    // ─────────────────────────────────────────────────────────────────
    // Replay-safety / idempotency
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Suspended_Is_Idempotent_On_Same_Event_Id()
    {
        var (handler, store) = NewHandler();
        await SeedStartedAsync(handler);

        var eventId = Guid.NewGuid();
        var schema = new WorkflowExecutionSuspendedEventSchema(Agg, "Approve", HumanApproval);

        await handler.HandleAsync(Envelope(eventId, schema));
        var first = await store.GetAsync(Agg);

        await handler.HandleAsync(Envelope(eventId, schema));
        var second = await store.GetAsync(Agg);

        Assert.Equal(first!.LastEventId, second!.LastEventId);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.ApprovalState, second.ApprovalState);
    }

    [Fact]
    public async Task Suspended_For_Unknown_Workflow_Is_Noop_Not_A_Throw()
    {
        var (handler, store) = NewHandler();
        // No Started event projected — envelope arrives out of order / on fresh store.

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionSuspendedEventSchema(Agg, "Approve", HumanApproval)));

        Assert.Null(await store.GetAsync(Agg));
    }

    [Fact]
    public async Task Cancelled_For_Unknown_Workflow_Is_Noop_Not_A_Throw()
    {
        var (handler, store) = NewHandler();

        await handler.HandleAsync(Envelope(Guid.NewGuid(),
            new WorkflowExecutionCancelledEventSchema(Agg, "Approve", $"{HumanApprovalRejected}:x:y:z")));

        Assert.Null(await store.GetAsync(Agg));
    }

    // ─────────────────────────────────────────────────────────────────
    // Test envelope
    // ─────────────────────────────────────────────────────────────────

    private sealed class FakeEnvelope : IEventEnvelope
    {
        public Guid EventId { get; init; }
        public Guid AggregateId { get; init; }
        public Guid CorrelationId { get; init; } = Guid.Empty;
        public Guid CausationId { get; init; } = Guid.Empty;
        public string EventType { get; init; } = string.Empty;
        public string EventName { get; init; } = string.Empty;
        public string EventVersion { get; init; } = string.Empty;
        public string SchemaHash { get; init; } = string.Empty;
        public object Payload { get; init; } = new();
        public string ExecutionHash { get; init; } = string.Empty;
        public string PolicyHash { get; init; } = string.Empty;
        public string? PolicyVersion { get; init; }
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UnixEpoch;
        public int SequenceNumber { get; init; }
        public string Classification { get; init; } = string.Empty;
        public string Context { get; init; } = string.Empty;
        public string Domain { get; init; } = string.Empty;
    }
}
