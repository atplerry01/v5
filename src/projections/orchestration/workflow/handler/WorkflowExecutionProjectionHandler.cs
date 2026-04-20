using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Orchestration.Workflow;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Projections.Orchestration.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Projections.Orchestration.Workflow;

/// <summary>
/// Materializes the workflow execution read model from lifecycle events.
/// Replaces the deprecated runtime-side WorkflowStateObserver: lifecycle
/// transitions are now domain events that flow through the runtime
/// persist → chain → outbox pipeline and arrive here as projection updates.
///
/// Implements <see cref="IEnvelopeProjectionHandler"/> so the file does not
/// reference src/runtime/** (projection guard PART D / DG-R7-01).
///
/// R3.A.6 / R-WF-APPROVAL-PROJ-01: the handler now reduces
/// <see cref="WorkflowExecutionSuspendedEventSchema"/> and
/// <see cref="WorkflowExecutionCancelledEventSchema"/>. Canonical
/// lifecycle <c>Status</c> is preserved (<c>Suspended</c>, <c>Cancelled</c>);
/// approval semantics are exposed through the derived
/// <see cref="WorkflowExecutionReadModel.ApprovalState"/> /
/// <see cref="WorkflowExecutionReadModel.ApprovalSignal"/> /
/// <see cref="WorkflowExecutionReadModel.ApprovalDecision"/> fields so
/// "Rejected" stays approval context and never overloads the
/// authoritative lifecycle status enum.
/// </summary>
public sealed class WorkflowExecutionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<WorkflowExecutionStartedEventSchema>,
    IProjectionHandler<WorkflowStepCompletedEventSchema>,
    IProjectionHandler<WorkflowExecutionCompletedEventSchema>,
    IProjectionHandler<WorkflowExecutionFailedEventSchema>,
    IProjectionHandler<WorkflowExecutionResumedEventSchema>,
    IProjectionHandler<WorkflowExecutionSuspendedEventSchema>,
    IProjectionHandler<WorkflowExecutionCancelledEventSchema>
{
    // R3.A.6 — canonical derived ApprovalState values surfaced on the read model.
    private const string ApprovalStateAwaiting = "AwaitingApproval";
    private const string ApprovalStateGranted = "Granted";
    private const string ApprovalStateRejected = "Rejected";
    private readonly IWorkflowExecutionProjectionStore _store;

    // phase1-gate-projection-hardening: per-envelope event id, captured by
    // HandleAsync(IEventEnvelope) and consumed by the per-type handlers for
    // idempotency-by-event-id. Safe under Inline execution policy because each
    // envelope is processed to completion before the next is dispatched.
    private Guid _currentEventId = Guid.Empty;

    public WorkflowExecutionProjectionHandler(IWorkflowExecutionProjectionStore store)
    {
        _store = store;
    }

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    // phase1.5-S5.2.3 / TC-6 (PROJECTION-CT-CONTRACT-01): the envelope
    // handler now consumes the worker's stoppingToken and forwards it
    // through every per-type overload. The IWorkflowExecutionProjectionStore
    // contract is unchanged in this pass — store-level token threading
    // is a separate workstream — so the per-type overloads accept the
    // token but do not yet forward it into the store calls.
    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _currentEventId = envelope.EventId;
        return envelope.Payload switch
        {
            WorkflowExecutionStartedEventSchema started => HandleAsync(started, cancellationToken),
            WorkflowStepCompletedEventSchema stepCompleted => HandleAsync(stepCompleted, cancellationToken),
            WorkflowExecutionCompletedEventSchema completed => HandleAsync(completed, cancellationToken),
            WorkflowExecutionFailedEventSchema failed => HandleAsync(failed, cancellationToken),
            WorkflowExecutionResumedEventSchema resumed => HandleAsync(resumed, cancellationToken),
            WorkflowExecutionSuspendedEventSchema suspended => HandleAsync(suspended, cancellationToken),
            WorkflowExecutionCancelledEventSchema cancelled => HandleAsync(cancelled, cancellationToken),
            _ => throw new InvalidOperationException(
                $"WorkflowExecutionProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(WorkflowExecutionStartedEventSchema e, CancellationToken cancellationToken = default)
    {
        // phase1-gate-projection-hardening: idempotency guard. Same-event
        // replay is a no-op.
        var existing = await _store.GetAsync(e.AggregateId);
        if (existing is not null && existing.LastEventId == _currentEventId) return;

        await _store.UpsertAsync(new WorkflowExecutionReadModel
        {
            WorkflowExecutionId = e.AggregateId,
            WorkflowName = e.WorkflowName,
            CurrentStepIndex = 0,
            ExecutionHash = string.Empty,
            Status = "Running",
            Payload = e.Payload,
            LastEventId = _currentEventId
        });
    }

    public async Task HandleAsync(WorkflowStepCompletedEventSchema e, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync(e.AggregateId);
        // phase1-gate-projection-hardening (E2): replay-safe — log-and-skip
        // when prior state is missing rather than throwing. Projection is
        // reflection, not reconstruction; do not fabricate stub rows. Per
        // H7a per-aggregate Kafka ordering, this should not occur in steady
        // state — the remaining cases are operational anomalies (offset
        // reset, store wipe, partial restore) which the projection must
        // tolerate without crashing the consumer.
        if (existing is null) return;

        // phase1-gate-projection-hardening: idempotency guard.
        if (existing.LastEventId == _currentEventId) return;

        // phase1-gate-projection-hardening (#14): construct a new dictionary
        // instead of mutating the existing one. The store now returns
        // defensive copies but this construction is the contract guarantee
        // — the handler must never mutate state it received from the store.
        var nextStepOutputs = new Dictionary<string, object?>(existing.StepOutputs)
        {
            [e.StepName] = e.Output
        };

        await _store.UpsertAsync(existing with
        {
            StepOutputs = nextStepOutputs,
            CurrentStepIndex = e.StepIndex,
            ExecutionHash = e.ExecutionHash,
            LastEventId = _currentEventId
        });
    }

    public async Task HandleAsync(WorkflowExecutionCompletedEventSchema e, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync(e.AggregateId);
        if (existing is null) return;
        if (existing.LastEventId == _currentEventId) return;

        await _store.UpsertAsync(existing with
        {
            ExecutionHash = e.ExecutionHash,
            Status = "Completed",
            LastEventId = _currentEventId
        });
    }

    public async Task HandleAsync(WorkflowExecutionFailedEventSchema e, CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync(e.AggregateId);
        if (existing is null) return;
        if (existing.LastEventId == _currentEventId) return;

        await _store.UpsertAsync(existing with
        {
            Status = "Failed",
            FailedStepName = e.FailedStepName,
            FailureReason = e.Reason,
            LastEventId = _currentEventId
        });
    }

    public async Task HandleAsync(WorkflowExecutionResumedEventSchema e, CancellationToken cancellationToken = default)
    {
        // phase1.6-stabilization S0.1: resume transitions the read model from
        // Failed back to Running. The failure context (step + reason) is
        // preserved in the event log; the read model represents *current*
        // state, so failed-step / failure-reason are cleared. Same E2 log-and-
        // skip + idempotency guards as the other handlers.
        var existing = await _store.GetAsync(e.AggregateId);
        if (existing is null) return;
        if (existing.LastEventId == _currentEventId) return;

        // R3.A.6 / R-WF-APPROVAL-PROJ-01: when the PreviousFailureReason
        // carries the canonical human_approval_granted prefix, surface
        // Granted on ApprovalState and clear ApprovalDecision. Other
        // resume carriers (failure-retry) leave ApprovalState null.
        var isApprovalGranted = StartsWithPrefix(
            e.PreviousFailureReason, WorkflowApprovalErrors.HumanApprovalGrantedPrefix);
        var approvalState = isApprovalGranted ? ApprovalStateGranted : existing.ApprovalState;
        var approvalSignal = isApprovalGranted
            ? ExtractPositionalSegment(e.PreviousFailureReason, WorkflowApprovalErrors.HumanApprovalGrantedPrefix, 0)
            : existing.ApprovalSignal;

        await _store.UpsertAsync(existing with
        {
            Status = "Running",
            FailedStepName = null,
            FailureReason = null,
            ApprovalState = approvalState,
            ApprovalSignal = approvalSignal,
            ApprovalDecision = null,
            LastEventId = _currentEventId
        });
    }

    public async Task HandleAsync(WorkflowExecutionSuspendedEventSchema e, CancellationToken cancellationToken = default)
    {
        // R3.A.6 / R-WF-APPROVAL-PROJ-01: reduce Suspended into the
        // canonical lifecycle Status = "Suspended" and derive approval
        // context from the carrier prefix. Non-approval suspends
        // (timer, external dependency — future phases) leave
        // ApprovalState null.
        var existing = await _store.GetAsync(e.AggregateId);
        if (existing is null) return;
        if (existing.LastEventId == _currentEventId) return;

        var isApprovalSuspend = IsHumanApprovalSignal(e.Reason);
        var approvalState = isApprovalSuspend ? ApprovalStateAwaiting : existing.ApprovalState;
        var approvalSignal = isApprovalSuspend
            ? ExtractSignalSuffix(e.Reason, WorkflowApprovalErrors.HumanApprovalPrefix)
            : existing.ApprovalSignal;

        await _store.UpsertAsync(existing with
        {
            Status = "Suspended",
            ApprovalState = approvalState,
            ApprovalSignal = approvalSignal,
            LastEventId = _currentEventId
        });
    }

    public async Task HandleAsync(WorkflowExecutionCancelledEventSchema e, CancellationToken cancellationToken = default)
    {
        // R3.A.6 / R-WF-APPROVAL-PROJ-01: reduce Cancelled into the
        // canonical lifecycle Status = "Cancelled". When the carrier
        // starts with human_approval_rejected, surface Rejected on
        // ApprovalState plus the parsed rationale suffix. Caller-
        // cancellation (the legacy R3.A.4 path) leaves ApprovalState
        // null — the terminal state is Cancelled regardless of cause.
        var existing = await _store.GetAsync(e.AggregateId);
        if (existing is null) return;
        if (existing.LastEventId == _currentEventId) return;

        var isApprovalReject = StartsWithPrefix(e.Reason, WorkflowApprovalErrors.HumanApprovalRejectedPrefix);
        var approvalState = isApprovalReject ? ApprovalStateRejected : existing.ApprovalState;
        var approvalSignal = isApprovalReject
            ? ExtractPositionalSegment(e.Reason, WorkflowApprovalErrors.HumanApprovalRejectedPrefix, 0)
            : existing.ApprovalSignal;
        var approvalDecision = isApprovalReject
            ? ExtractPositionalSegment(e.Reason, WorkflowApprovalErrors.HumanApprovalRejectedPrefix, 2)
            : existing.ApprovalDecision;

        await _store.UpsertAsync(existing with
        {
            Status = "Cancelled",
            ApprovalState = approvalState,
            ApprovalSignal = approvalSignal,
            ApprovalDecision = approvalDecision,
            LastEventId = _currentEventId
        });
    }

    private static bool StartsWithPrefix(string value, string prefix)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (value == prefix) return true;
        return value.StartsWith(prefix + ":", StringComparison.Ordinal);
    }

    private static bool IsHumanApprovalSignal(string reason)
    {
        // Matches bare "human_approval" OR "human_approval:<signal>..."
        // but NOT "human_approval_granted" / "human_approval_rejected"
        // prefixes (those belong on Resumed / Cancelled respectively).
        if (string.IsNullOrEmpty(reason)) return false;
        if (reason == WorkflowApprovalErrors.HumanApprovalPrefix) return true;
        return reason.StartsWith(WorkflowApprovalErrors.HumanApprovalPrefix + ":", StringComparison.Ordinal);
    }

    private static string? ExtractSignalSuffix(string reason, string prefix)
    {
        if (string.IsNullOrEmpty(reason)) return null;
        var withColon = prefix + ":";
        if (!reason.StartsWith(withColon, StringComparison.Ordinal)) return null;
        var rest = reason[withColon.Length..];
        // Suspended carrier is just "{prefix}:{signal}" — no further segments.
        return string.IsNullOrEmpty(rest) ? null : rest;
    }

    /// <summary>
    /// Extract the <paramref name="segmentIndex"/>-th colon-separated segment
    /// following <paramref name="prefix"/>. Granted carrier:
    /// <c>{prefix}:{signal}:{actor}[:{rationale}]</c>. Rejected carrier:
    /// <c>{prefix}:{signal}:{actor}[:{rationale}]</c>. segmentIndex=0 =
    /// signal, 1 = actor (non-authoritative), 2 = rationale. Returns
    /// null when the segment is absent or empty.
    /// </summary>
    private static string? ExtractPositionalSegment(string reason, string prefix, int segmentIndex)
    {
        if (string.IsNullOrEmpty(reason)) return null;
        var withColon = prefix + ":";
        if (!reason.StartsWith(withColon, StringComparison.Ordinal)) return null;
        var rest = reason[withColon.Length..];
        var segments = rest.Split(':');
        if (segmentIndex < 0 || segmentIndex >= segments.Length) return null;
        var value = segments[segmentIndex];
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
