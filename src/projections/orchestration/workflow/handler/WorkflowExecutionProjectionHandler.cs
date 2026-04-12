using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Orchestration.Workflow;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Projection;
using Whyce.Shared.Contracts.Projections.Orchestration.Workflow;

namespace Whyce.Projections.Orchestration.Workflow;

/// <summary>
/// Materializes the workflow execution read model from lifecycle events.
/// Replaces the deprecated runtime-side WorkflowStateObserver: lifecycle
/// transitions are now domain events that flow through the runtime
/// persist → chain → outbox pipeline and arrive here as projection updates.
///
/// Implements <see cref="IEnvelopeProjectionHandler"/> so the file does not
/// reference src/runtime/** (projection guard PART D / DG-R7-01).
/// </summary>
public sealed class WorkflowExecutionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<WorkflowExecutionStartedEventSchema>,
    IProjectionHandler<WorkflowStepCompletedEventSchema>,
    IProjectionHandler<WorkflowExecutionCompletedEventSchema>,
    IProjectionHandler<WorkflowExecutionFailedEventSchema>,
    IProjectionHandler<WorkflowExecutionResumedEventSchema>
{
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

        await _store.UpsertAsync(existing with
        {
            Status = "Running",
            FailedStepName = null,
            FailureReason = null,
            LastEventId = _currentEventId
        });
    }
}
