using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Engines.T1M.Core.Lifecycle;

/// <summary>
/// T1M-tier factory that produces workflow lifecycle domain events directly,
/// without invoking mutating methods on <see cref="WorkflowExecutionAggregate"/>.
///
/// Engine-guard rule 3 prohibits T1M from calling aggregate mutation methods.
/// The factory satisfies that constraint by constructing the events itself.
/// The aggregate remains the canonical replay target (its Apply method
/// reconstructs state from these same events).
///
/// The factory consults <see cref="IPayloadTypeRegistry"/> to stamp the
/// PayloadType / OutputType discriminators on lifecycle events when the
/// payload's CLR type has been registered by a domain bootstrap module. The
/// discriminator is what allows the engine-side replay service to rehydrate
/// the typed CLR object from JSON on Postgres-backed replay (rule E-TYPE-02).
/// </summary>
public sealed class WorkflowLifecycleEventFactory
{
    private readonly IPayloadTypeRegistry _payloadTypes;

    public WorkflowLifecycleEventFactory(IPayloadTypeRegistry payloadTypes)
    {
        _payloadTypes = payloadTypes;
    }

    public WorkflowExecutionStartedEvent Started(
        Guid workflowExecutionId, string workflowName, object? payload = null)
        => new(new AggregateId(workflowExecutionId), workflowName, payload, ResolveTypeName(payload));

    public WorkflowStepCompletedEvent StepCompleted(
        Guid workflowExecutionId, int stepIndex, string stepName, string executionHash, object? output = null)
        => new(new AggregateId(workflowExecutionId), stepIndex, stepName, executionHash, output, ResolveTypeName(output));

    private string? ResolveTypeName(object? value)
    {
        if (value is null) return null;
        return _payloadTypes.TryGetName(value.GetType(), out var name) ? name : null;
    }

    public WorkflowExecutionCompletedEvent Completed(Guid workflowExecutionId, string executionHash)
        => new(new AggregateId(workflowExecutionId), executionHash);

    public WorkflowExecutionFailedEvent Failed(
        Guid workflowExecutionId, string failedStepName, string reason)
        => new(new AggregateId(workflowExecutionId), failedStepName, reason);

    /// <summary>
    /// phase1.6-S1.2 (E-LIFECYCLE-FACTORY-CALL-SITE-01): canonical construction
    /// site for <see cref="WorkflowExecutionResumedEvent"/>. Replaces the
    /// previous <c>WorkflowExecutionAggregate.Resume()</c> mutation method —
    /// the aggregate no longer exposes a Resume command. State change happens
    /// only when the runtime persist pipeline replays this event back through
    /// <c>Apply(WorkflowExecutionResumedEvent)</c>.
    ///
    /// The factory enforces the same precondition the aggregate previously
    /// guarded (status MUST be Failed), reading the failure context from the
    /// aggregate's public surface. The aggregate is not mutated; on success
    /// the caller appends the returned event to the event store and the
    /// next replay reconstructs the Running status via Apply.
    /// </summary>
    public WorkflowExecutionResumedEvent Resumed(WorkflowExecutionAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        // R3.A.3 / R-WORKFLOW-SUSPEND-RESUME-GUARD-01: resume is legal
        // from BOTH Failed and Suspended. Terminal statuses
        // (Completed, Cancelled) remain un-resumable.
        Guard.Against(
            aggregate.Status != WorkflowExecutionStatus.Failed
                && aggregate.Status != WorkflowExecutionStatus.Suspended,
            WorkflowExecutionErrors.CannotResumeUnlessFailedOrSuspended);

        return new WorkflowExecutionResumedEvent(
            new AggregateId(aggregate.Id.Value),
            aggregate.FailedStepName ?? string.Empty,
            aggregate.FailureReason ?? string.Empty);
    }

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-03 — approval-granted resume. Same
    /// precondition as the failure-resume overload (legal from Failed OR
    /// Suspended) but allows the caller to override
    /// <c>PreviousFailureReason</c> with a composed
    /// <c>human_approval_granted:{signal}:{actor}</c> carrier.
    ///
    /// Temporary carrier per R3.A.6 D4: the <c>PreviousFailureReason</c>
    /// field on <see cref="WorkflowExecutionResumedEvent"/> is reused as
    /// an observability carrier for approval-granted context. It does
    /// NOT denote failure semantics when the prefix is
    /// <c>human_approval_granted</c>. Approval-granted is a successful
    /// approval decision; the field name is failure-coupled for
    /// historical reasons and will be normalized in a future pass.
    /// </summary>
    public WorkflowExecutionResumedEvent Resumed(
        WorkflowExecutionAggregate aggregate, string approvalSignalOverride)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentException.ThrowIfNullOrEmpty(approvalSignalOverride);

        Guard.Against(
            aggregate.Status != WorkflowExecutionStatus.Failed
                && aggregate.Status != WorkflowExecutionStatus.Suspended,
            WorkflowExecutionErrors.CannotResumeUnlessFailedOrSuspended);

        return new WorkflowExecutionResumedEvent(
            new AggregateId(aggregate.Id.Value),
            aggregate.FailedStepName ?? string.Empty,
            approvalSignalOverride);
    }

    /// <summary>
    /// R3.A.4 / R-WORKFLOW-CANCELLATION-FACTORY-01 — canonical
    /// construction site for <see cref="WorkflowExecutionCancelledEvent"/>.
    /// Called by <c>T1MWorkflowEngine</c> from the caller-cancellation
    /// catch branch BEFORE the propagating
    /// <see cref="System.OperationCanceledException"/> re-throws to
    /// the caller. Mirrors the <see cref="Resumed"/> shape —
    /// engine-guard rule 3 prohibits T1M from calling aggregate
    /// mutation methods, so the factory constructs the event and
    /// the runtime persist pipeline lands it via <c>Apply</c> on the
    /// next replay.
    ///
    /// <paramref name="stepName"/> is nullable because cancellation
    /// MAY arrive before any step began (the caller-cancel catch sits
    /// inside the step loop so the step is usually defined, but the
    /// nullability is preserved for forward compat with a future
    /// pre-step cancel surface).
    /// </summary>
    public WorkflowExecutionCancelledEvent Cancelled(
        Guid workflowExecutionId, string? stepName, string reason)
        => new(new AggregateId(workflowExecutionId), stepName, reason);

    /// <summary>
    /// R3.A.3 / R-WORKFLOW-SUSPEND-FACTORY-01 — canonical construction
    /// site for <see cref="WorkflowExecutionSuspendedEvent"/>. Guards
    /// the Running-only precondition: suspending from any other status
    /// (NotStarted / Completed / Failed / Cancelled / already-Suspended)
    /// is illegal.
    ///
    /// Symmetric with <see cref="Resumed"/> in discipline — engine-guard
    /// rule 3 prohibits T1M from calling aggregate mutation methods, so
    /// the factory constructs the event and the runtime persist pipeline
    /// lands it via <c>Apply</c> on the next replay. The aggregate has
    /// NO public <c>Suspend(...)</c> method.
    ///
    /// <paramref name="stepName"/> is nullable so workflow-level suspend
    /// (pre-first-step or post-last-step) is expressible alongside
    /// mid-step suspend. <paramref name="reason"/> is audit-only, not a
    /// replay discriminator — two streams that land at the same end
    /// state are replay-equivalent regardless of reason.
    /// </summary>
    public WorkflowExecutionSuspendedEvent Suspended(
        WorkflowExecutionAggregate aggregate, string? stepName, string reason)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        Guard.Against(
            aggregate.Status != WorkflowExecutionStatus.Running,
            WorkflowExecutionErrors.CannotSuspendUnlessRunning);

        return new WorkflowExecutionSuspendedEvent(
            new AggregateId(aggregate.Id.Value), stepName, reason);
    }

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-01 — engine-side Suspended emission. The
    /// T1M workflow engine does not have the aggregate in scope during
    /// <c>ExecuteAsync</c>; it operates on an in-flight execution
    /// context. The Running-state invariant is established by
    /// construction — this overload is reached only when a step returned
    /// <see cref="WorkflowStepResult.AwaitingApproval"/> inside the
    /// main execution loop, which implies the aggregate is currently
    /// Running. Analogous to the factory's
    /// <see cref="Failed(Guid, string, string)"/> and
    /// <see cref="Cancelled(Guid, string?, string)"/> overloads, which
    /// also carry no aggregate precondition because the engine enforces
    /// the state invariant by construction.
    ///
    /// Non-engine callers (operator surface, runtime direct suspend in
    /// future phases) MUST use the aggregate-taking overload so the
    /// precondition is checked.
    /// </summary>
    public WorkflowExecutionSuspendedEvent Suspended(
        Guid workflowExecutionId, string? stepName, string reason)
        => new(new AggregateId(workflowExecutionId), stepName, reason);
}
