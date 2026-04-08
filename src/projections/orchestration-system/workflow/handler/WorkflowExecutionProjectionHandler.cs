using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.OrchestrationSystem.Workflow;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Projection;
using Whyce.Shared.Contracts.Projections.OrchestrationSystem.Workflow;

namespace Whyce.Projections.OrchestrationSystem.Workflow;

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
    IProjectionHandler<WorkflowExecutionFailedEventSchema>
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

    public Task HandleAsync(IEventEnvelope envelope)
    {
        _currentEventId = envelope.EventId;
        return envelope.Payload switch
        {
            WorkflowExecutionStartedEventSchema started => HandleAsync(started),
            WorkflowStepCompletedEventSchema stepCompleted => HandleAsync(stepCompleted),
            WorkflowExecutionCompletedEventSchema completed => HandleAsync(completed),
            WorkflowExecutionFailedEventSchema failed => HandleAsync(failed),
            _ => throw new InvalidOperationException(
                $"WorkflowExecutionProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(WorkflowExecutionStartedEventSchema e)
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

    public async Task HandleAsync(WorkflowStepCompletedEventSchema e)
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

    public async Task HandleAsync(WorkflowExecutionCompletedEventSchema e)
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

    public async Task HandleAsync(WorkflowExecutionFailedEventSchema e)
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
}
