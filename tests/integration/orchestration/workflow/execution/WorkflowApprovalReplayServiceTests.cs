using Whycespace.Engines.T1M.Core.Lifecycle;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Integration.Orchestration.Workflow.Execution;

/// <summary>
/// R3.A.6 / R-WF-APPROVAL-03..04 coverage for
/// <see cref="IWorkflowExecutionReplayService.ResumeWithApprovalAsync"/>
/// and <see cref="IWorkflowExecutionReplayService.CancelSuspendedAsync"/>.
/// Asserts state preconditions, canonical human_approval* carrier
/// composition, authoritative-actor sourcing, and signal round-trip.
/// Self-contained (mutable event-store stub) so no infrastructure is
/// required.
/// </summary>
public sealed class WorkflowApprovalReplayServiceTests
{
    private const string HumanApproval = WorkflowApprovalErrors.HumanApprovalPrefix;
    private const string HumanApprovalGranted = WorkflowApprovalErrors.HumanApprovalGrantedPrefix;
    private const string HumanApprovalRejected = WorkflowApprovalErrors.HumanApprovalRejectedPrefix;

    private static readonly Guid ExecutionId = Guid.Parse("00000000-0000-0000-0000-000000000060");
    private const string Approver = "actor/operator-42";
    private const string Signal = "risk-sign-off";

    private static (WorkflowExecutionReplayService service, MutableEventStore store) NewService()
    {
        var store = new MutableEventStore();
        var registry = new PayloadTypeRegistry();
        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));
        return (service, store);
    }

    // ─────────────────────────────────────────────────────────────────
    // ResumeWithApprovalAsync — precondition failures
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResumeWithApproval_Throws_When_No_Events()
    {
        var (service, _) = NewService();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ResumeWithApprovalAsync(ExecutionId, Approver));
    }

    [Fact]
    public async Task ResumeWithApproval_Throws_When_Not_Suspended()
    {
        var (service, store) = NewService();
        var agg = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        agg.CompleteStep(0, "Validate", "h0");
        store.Append(agg.DomainEvents); // Running, not Suspended

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ResumeWithApprovalAsync(ExecutionId, Approver));
        Assert.Equal(WorkflowApprovalErrors.CannotApproveUnlessAwaitingApproval, ex.Message);
    }

    [Fact]
    public async Task ResumeWithApproval_Throws_When_Failed_Not_Suspended()
    {
        var (service, store) = NewService();
        var agg = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        agg.Fail("Charge", "card declined");
        store.Append(agg.DomainEvents); // Failed, not Suspended

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ResumeWithApprovalAsync(ExecutionId, Approver));
        Assert.Equal(WorkflowApprovalErrors.CannotApproveUnlessAwaitingApproval, ex.Message);
    }

    [Fact]
    public async Task ResumeWithApproval_Throws_When_Suspended_With_Non_Approval_Signal()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, suspendReason: "timer_wait:30s");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ResumeWithApprovalAsync(ExecutionId, Approver));
        Assert.Equal(WorkflowApprovalErrors.CannotApproveUnlessAwaitingApproval, ex.Message);
    }

    [Fact]
    public async Task ResumeWithApproval_Throws_When_Approver_Empty()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ResumeWithApprovalAsync(ExecutionId, ""));
    }

    [Fact]
    public async Task ResumeWithApproval_Throws_When_Approver_Whitespace()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ResumeWithApprovalAsync(ExecutionId, "   "));
    }

    // ─────────────────────────────────────────────────────────────────
    // ResumeWithApprovalAsync — happy path + carrier composition
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResumeWithApproval_Emits_Resumed_With_Granted_Prefix()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store);

        var resumed = (WorkflowExecutionResumedEvent)await service.ResumeWithApprovalAsync(ExecutionId, Approver);

        Assert.StartsWith(HumanApprovalGranted + ":", resumed.PreviousFailureReason);
    }

    [Fact]
    public async Task ResumeWithApproval_Preserves_Signal_From_Suspended_Event()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, signal: Signal);

        var resumed = (WorkflowExecutionResumedEvent)await service.ResumeWithApprovalAsync(ExecutionId, Approver);

        // Carrier shape: "human_approval_granted:{signal}:{actor}"
        Assert.Contains($":{Signal}:", resumed.PreviousFailureReason);
    }

    [Fact]
    public async Task ResumeWithApproval_Includes_Authoritative_Actor()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, signal: Signal);

        var resumed = (WorkflowExecutionResumedEvent)await service.ResumeWithApprovalAsync(ExecutionId, Approver);

        Assert.Contains(Approver, resumed.PreviousFailureReason);
    }

    [Fact]
    public async Task ResumeWithApproval_Includes_Rationale_Suffix_When_Provided()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, signal: Signal);

        var resumed = (WorkflowExecutionResumedEvent)await service.ResumeWithApprovalAsync(
            ExecutionId, Approver, rationale: "CFO-approved");

        Assert.EndsWith(":CFO-approved", resumed.PreviousFailureReason);
    }

    [Fact]
    public async Task ResumeWithApproval_Handles_Bare_Human_Approval_Prefix_Without_Signal()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, suspendReason: HumanApproval); // bare, no signal suffix

        var resumed = (WorkflowExecutionResumedEvent)await service.ResumeWithApprovalAsync(ExecutionId, Approver);

        Assert.StartsWith(HumanApprovalGranted + ":", resumed.PreviousFailureReason);
        // signal segment is empty but the positional structure is preserved.
        Assert.Contains($"::{Approver}", resumed.PreviousFailureReason);
    }

    // ─────────────────────────────────────────────────────────────────
    // CancelSuspendedAsync — precondition failures
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelSuspended_Throws_When_No_Events()
    {
        var (service, _) = NewService();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CancelSuspendedAsync(ExecutionId, Approver));
    }

    [Fact]
    public async Task CancelSuspended_Throws_When_Not_Suspended()
    {
        var (service, store) = NewService();
        var agg = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        store.Append(agg.DomainEvents); // Running

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CancelSuspendedAsync(ExecutionId, Approver));
        Assert.Equal(WorkflowApprovalErrors.CannotRejectUnlessAwaitingApproval, ex.Message);
    }

    [Fact]
    public async Task CancelSuspended_Throws_When_Suspended_With_Non_Approval_Signal()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, suspendReason: "timer_wait:10s");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CancelSuspendedAsync(ExecutionId, Approver));
        Assert.Equal(WorkflowApprovalErrors.CannotRejectUnlessAwaitingApproval, ex.Message);
    }

    [Fact]
    public async Task CancelSuspended_Throws_When_Approver_Empty()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CancelSuspendedAsync(ExecutionId, ""));
    }

    // ─────────────────────────────────────────────────────────────────
    // CancelSuspendedAsync — happy path + carrier composition
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelSuspended_Emits_Cancelled_With_Rejected_Prefix()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, signal: Signal);

        var cancelled = (WorkflowExecutionCancelledEvent)await service.CancelSuspendedAsync(ExecutionId, Approver);

        Assert.StartsWith(HumanApprovalRejected + ":", cancelled.Reason);
    }

    [Fact]
    public async Task CancelSuspended_Inherits_StepName_From_Suspended_Event()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, signal: Signal, stepName: "Review");

        var cancelled = (WorkflowExecutionCancelledEvent)await service.CancelSuspendedAsync(ExecutionId, Approver);

        Assert.Equal("Review", cancelled.StepName);
    }

    [Fact]
    public async Task CancelSuspended_Includes_Rationale_Suffix_When_Provided()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, signal: Signal);

        var cancelled = (WorkflowExecutionCancelledEvent)await service.CancelSuspendedAsync(
            ExecutionId, Approver, rationale: "Risk-exceeded");

        Assert.EndsWith(":Risk-exceeded", cancelled.Reason);
    }

    [Fact]
    public async Task CancelSuspended_Preserves_Signal_And_Actor_Segments()
    {
        var (service, store) = NewService();
        SeedSuspendedWorkflow(store, signal: Signal);

        var cancelled = (WorkflowExecutionCancelledEvent)await service.CancelSuspendedAsync(
            ExecutionId, Approver);

        Assert.Contains($":{Signal}:{Approver}", cancelled.Reason);
    }

    // ─────────────────────────────────────────────────────────────────
    // Seeding helpers
    // ─────────────────────────────────────────────────────────────────

    private static void SeedSuspendedWorkflow(
        MutableEventStore store,
        string? suspendReason = null,
        string? signal = null,
        string stepName = "Approve")
    {
        var agg = WorkflowExecutionAggregate.Start(new WorkflowExecutionId(ExecutionId), "wf");
        store.Append(agg.DomainEvents);

        // Append a Suspended event directly — the aggregate Apply handles the state transition.
        var reason = suspendReason
            ?? (signal is null ? HumanApproval : $"{HumanApproval}:{signal}");
        var suspended = new WorkflowExecutionSuspendedEvent(
            new AggregateId(ExecutionId), stepName, reason);
        store.Append(new object[] { suspended });
    }

    private sealed class MutableEventStore : IEventStore
    {
        private readonly List<object> _events = [];

        public Task<IReadOnlyList<object>> LoadEventsAsync(
            Guid aggregateId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<object>>(_events.ToList());

        public Task AppendEventsAsync(
            Guid aggregateId,
            IReadOnlyList<IEventEnvelope> envelopes,
            int expectedVersion,
            CancellationToken cancellationToken = default)
        {
            _events.AddRange(envelopes.Select(e => e.Payload));
            return Task.CompletedTask;
        }

        public void Append(IEnumerable<object> events) => _events.AddRange(events);
    }
}
