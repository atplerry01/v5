using Whyce.Shared.Contracts.Events.OrchestrationSystem.Workflow;
using DomainEvents = Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

namespace Whyce.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the orchestration/workflow/execution lifecycle.
///
/// Owns the binding from workflow execution domain events to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events into shared schema records. Relocated from
/// <c>src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs</c>
/// under Phase 1.5 §5.1.2 BPV-D01.
/// </summary>
public sealed class WorkflowExecutionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "WorkflowExecutionStartedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionStartedEvent),
            typeof(WorkflowExecutionStartedEventSchema));
        sink.RegisterSchema(
            "WorkflowStepCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowStepCompletedEvent),
            typeof(WorkflowStepCompletedEventSchema));
        sink.RegisterSchema(
            "WorkflowExecutionCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionCompletedEvent),
            typeof(WorkflowExecutionCompletedEventSchema));
        sink.RegisterSchema(
            "WorkflowExecutionFailedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionFailedEvent),
            typeof(WorkflowExecutionFailedEventSchema));
        sink.RegisterSchema(
            "WorkflowExecutionResumedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionResumedEvent),
            typeof(WorkflowExecutionResumedEventSchema));

        sink.RegisterPayloadMapper("WorkflowExecutionStartedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionStartedEvent)e;
            return new WorkflowExecutionStartedEventSchema(evt.AggregateId.Value, evt.WorkflowName, evt.Payload);
        });
        sink.RegisterPayloadMapper("WorkflowStepCompletedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowStepCompletedEvent)e;
            return new WorkflowStepCompletedEventSchema(
                evt.AggregateId.Value, evt.StepIndex, evt.StepName, evt.ExecutionHash, evt.Output);
        });
        sink.RegisterPayloadMapper("WorkflowExecutionCompletedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionCompletedEvent)e;
            return new WorkflowExecutionCompletedEventSchema(evt.AggregateId.Value, evt.ExecutionHash);
        });
        sink.RegisterPayloadMapper("WorkflowExecutionFailedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionFailedEvent)e;
            return new WorkflowExecutionFailedEventSchema(
                evt.AggregateId.Value, evt.FailedStepName, evt.Reason);
        });
        sink.RegisterPayloadMapper("WorkflowExecutionResumedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionResumedEvent)e;
            return new WorkflowExecutionResumedEventSchema(
                evt.AggregateId.Value, evt.ResumedFromStepName, evt.PreviousFailureReason);
        });
    }
}
