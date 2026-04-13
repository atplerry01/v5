using System.Text.Json;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

namespace Whycespace.Engines.T1M.Core.Lifecycle;

/// <summary>
/// Reconstructs <see cref="WorkflowExecutionAggregate"/> deterministically
/// from the event store and projects it onto the runtime-facing
/// <see cref="WorkflowExecutionReplayState"/> DTO.
///
/// This is the engine-side implementation of
/// <see cref="IWorkflowExecutionReplayService"/>; it lives under
/// src/engines/T1M/lifecycle/ so it can reference
/// Whycespace.Domain.OrchestrationSystem.Workflow.Execution.* without
/// violating runtime.guard R-DOM-01. The runtime dispatcher depends only
/// on the shared contract.
///
/// Determinism: replay folds events through the aggregate's existing
/// LoadFromHistory pathway — no clock, no Guid.NewGuid, no policy
/// re-evaluation, no sentinel writes. Protected by
/// replay-determinism.guard (REPLAY-SENTINEL-PROTECTED-01).
/// </summary>
public sealed class WorkflowExecutionReplayService : IWorkflowExecutionReplayService
{
    private readonly IEventStore _eventStore;
    private readonly IPayloadTypeRegistry _payloadTypes;
    private readonly WorkflowLifecycleEventFactory _lifecycleFactory;

    public WorkflowExecutionReplayService(
        IEventStore eventStore,
        IPayloadTypeRegistry payloadTypes,
        WorkflowLifecycleEventFactory lifecycleFactory)
    {
        _eventStore = eventStore;
        _payloadTypes = payloadTypes;
        _lifecycleFactory = lifecycleFactory;
    }

    public async Task<WorkflowExecutionReplayState?> ReplayAsync(Guid workflowExecutionId)
    {
        var events = await _eventStore.LoadEventsAsync(workflowExecutionId);
        if (events.Count == 0)
        {
            return null;
        }

        var aggregate = (WorkflowExecutionAggregate)System.Activator.CreateInstance(
            typeof(WorkflowExecutionAggregate),
            nonPublic: true)!;
        aggregate.LoadFromHistory(events);

        // NextStepIndex = number of completed step events. Unambiguous and
        // event-derived; do not use aggregate.CurrentStepIndex which collapses
        // "started, no steps" and "step 0 completed" to the same value.
        // Payload + StepOutputs are extracted from the same pass.
        //
        // Typed-payload rehydration (rule E-TYPE-03): when an event was
        // round-tripped through PostgresEventStoreAdapter, the static
        // `object?` typing causes Payload/Output to come back as JsonElement.
        // If the event also carries a PayloadType/OutputType discriminator
        // (stamped by WorkflowLifecycleEventFactory at write time, see
        // E-TYPE-02), we re-deserialize via IPayloadTypeRegistry to restore
        // the original CLR type. In-process / in-memory replay preserves the
        // original CLR reference and skips the JsonElement branch.
        var nextStepIndex = 0;
        object? payload = null;
        var stepOutputs = new Dictionary<string, object?>();
        for (var i = 0; i < events.Count; i++)
        {
            switch (events[i])
            {
                case WorkflowExecutionStartedEvent started:
                    payload = Rehydrate(started.Payload, started.PayloadType);
                    break;
                case WorkflowStepCompletedEvent step:
                    nextStepIndex++;
                    stepOutputs[step.StepName] = Rehydrate(step.Output, step.OutputType);
                    break;
            }
        }

        return new WorkflowExecutionReplayState(
            WorkflowExecutionId: aggregate.Id.Value,
            WorkflowName: aggregate.WorkflowName,
            NextStepIndex: nextStepIndex,
            ExecutionHash: aggregate.ExecutionHash,
            Status: aggregate.Status.ToString(),
            Payload: payload,
            StepOutputs: stepOutputs);
    }

    public async Task<object> ResumeAsync(Guid workflowExecutionId)
    {
        var events = await _eventStore.LoadEventsAsync(workflowExecutionId);
        if (events.Count == 0)
            throw new InvalidOperationException(
                $"Workflow execution '{workflowExecutionId}' has no recorded events; cannot resume.");

        var aggregate = (WorkflowExecutionAggregate)System.Activator.CreateInstance(
            typeof(WorkflowExecutionAggregate),
            nonPublic: true)!;
        aggregate.LoadFromHistory(events);

        if (aggregate.Status != WorkflowExecutionStatus.Failed)
            throw new InvalidOperationException(
                $"Workflow execution '{workflowExecutionId}' is in status {aggregate.Status}; " +
                "resume is only valid from the Failed state.");

        // phase1.6-S1.2 (E-LIFECYCLE-FACTORY-CALL-SITE-01): construct the
        // resume event via the lifecycle factory instead of mutating the
        // aggregate. The factory re-validates the Failed precondition, reads
        // the failure context from the aggregate's public surface, and
        // returns the event without touching aggregate state. The runtime
        // dispatcher then appends it to the workflow's accumulated events,
        // the persist pipeline writes it to the event store, and the next
        // replay reconstructs the Running status via Apply.
        return _lifecycleFactory.Resumed(aggregate);
    }

    private object? Rehydrate(object? value, string? typeName)
    {
        // Null payloads remain null — there is nothing to type and no
        // JsonElement to leak.
        if (value is null) return null;

        // In-memory / in-process replay: the value is already a typed CLR
        // object. No rehydration is required and a missing typeName is
        // tolerated because there is no JsonElement crossing the replay
        // boundary.
        if (value is not JsonElement json) return value;

        // Postgres-backed replay: the value came back as a JsonElement.
        // H10 strict mode — the typeName MUST be present and resolvable.
        // No silent fallback, no dynamic, no JsonElement leakage past this
        // boundary. (See claude/project-prompts/20260407-215052-orchestration-
        // system-h10-type-safety.md.)
        if (typeName is null)
            throw new InvalidOperationException(
                "H10 type safety violation: workflow event payload arrived as JsonElement " +
                "but carries no PayloadType/OutputType discriminator. " +
                "Register the CLR type via IPayloadTypeRegistry from the owning " +
                "IDomainBootstrapModule so the write-side factory stamps a discriminator.");

        var type = _payloadTypes.Resolve(typeName);
        return JsonSerializer.Deserialize(json.GetRawText(), type);
    }
}
