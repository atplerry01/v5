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

    public WorkflowExecutionProjectionHandler(IWorkflowExecutionProjectionStore store)
    {
        _store = store;
    }

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope) => envelope.Payload switch
    {
        WorkflowExecutionStartedEventSchema started => HandleAsync(started),
        WorkflowStepCompletedEventSchema stepCompleted => HandleAsync(stepCompleted),
        WorkflowExecutionCompletedEventSchema completed => HandleAsync(completed),
        WorkflowExecutionFailedEventSchema failed => HandleAsync(failed),
        _ => throw new InvalidOperationException(
            $"WorkflowExecutionProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}.")
    };

    public async Task HandleAsync(WorkflowExecutionStartedEventSchema e)
    {
        await _store.UpsertAsync(new WorkflowExecutionReadModel
        {
            WorkflowExecutionId = e.AggregateId,
            WorkflowName = e.WorkflowName,
            CurrentStepIndex = 0,
            ExecutionHash = string.Empty,
            Status = "Running",
            Payload = e.Payload
        });
    }

    public async Task HandleAsync(WorkflowStepCompletedEventSchema e)
    {
        var existing = await _store.GetAsync(e.AggregateId)
            ?? throw new InvalidOperationException(
                $"WorkflowExecutionReadModel not found for {e.AggregateId} on step completion.");

        // Mutate the dictionary in place — init-only on the property prevents
        // reassignment, not member mutation. Then upsert the (otherwise unchanged)
        // record reference with updated cursor + hash.
        existing.StepOutputs[e.StepName] = e.Output;

        await _store.UpsertAsync(existing with
        {
            CurrentStepIndex = e.StepIndex,
            ExecutionHash = e.ExecutionHash
        });
    }

    public async Task HandleAsync(WorkflowExecutionCompletedEventSchema e)
    {
        var existing = await _store.GetAsync(e.AggregateId)
            ?? throw new InvalidOperationException(
                $"WorkflowExecutionReadModel not found for {e.AggregateId} on completion.");

        await _store.UpsertAsync(existing with
        {
            ExecutionHash = e.ExecutionHash,
            Status = "Completed"
        });
    }

    public async Task HandleAsync(WorkflowExecutionFailedEventSchema e)
    {
        var existing = await _store.GetAsync(e.AggregateId)
            ?? throw new InvalidOperationException(
                $"WorkflowExecutionReadModel not found for {e.AggregateId} on failure.");

        await _store.UpsertAsync(existing with
        {
            Status = "Failed",
            FailedStepName = e.FailedStepName,
            FailureReason = e.Reason
        });
    }
}
