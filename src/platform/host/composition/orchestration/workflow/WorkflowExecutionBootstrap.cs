using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Adapters;
using Whyce.Projections.OrchestrationSystem.Workflow;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Events.OrchestrationSystem.Workflow;
using Whyce.Shared.Contracts.Projections.OrchestrationSystem.Workflow;
using Whyce.Shared.Contracts.Runtime;
using DomainEvents = Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

namespace Whyce.Platform.Host.Composition.Orchestration.Workflow;

/// <summary>
/// Bootstrap module for the workflow execution lifecycle. Owns:
/// - In-memory projection store registration (placeholder; see migration 002)
/// - Workflow lifecycle event schemas + payload mappers (domain → schema)
/// - WorkflowExecutionProjectionHandler registration in ProjectionRegistry
///
/// This module owns no engine or workflow definition — workflow execution
/// lifecycle is cross-domain plumbing, not a per-domain workflow.
/// </summary>
public sealed class WorkflowExecutionBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IWorkflowExecutionProjectionStore, InMemoryWorkflowExecutionProjectionStore>();
        services.AddSingleton<WorkflowExecutionProjectionHandler>();
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        schema.Register(
            "WorkflowExecutionStartedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionStartedEvent),
            typeof(WorkflowExecutionStartedEventSchema));
        schema.Register(
            "WorkflowStepCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowStepCompletedEvent),
            typeof(WorkflowStepCompletedEventSchema));
        schema.Register(
            "WorkflowExecutionCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionCompletedEvent),
            typeof(WorkflowExecutionCompletedEventSchema));
        schema.Register(
            "WorkflowExecutionFailedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionFailedEvent),
            typeof(WorkflowExecutionFailedEventSchema));
        schema.Register(
            "WorkflowExecutionResumedEvent",
            EventVersion.Default,
            typeof(DomainEvents.WorkflowExecutionResumedEvent),
            typeof(WorkflowExecutionResumedEventSchema));

        schema.RegisterPayloadMapper("WorkflowExecutionStartedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionStartedEvent)e;
            return new WorkflowExecutionStartedEventSchema(evt.AggregateId.Value, evt.WorkflowName, evt.Payload);
        });
        schema.RegisterPayloadMapper("WorkflowStepCompletedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowStepCompletedEvent)e;
            return new WorkflowStepCompletedEventSchema(
                evt.AggregateId.Value, evt.StepIndex, evt.StepName, evt.ExecutionHash, evt.Output);
        });
        schema.RegisterPayloadMapper("WorkflowExecutionCompletedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionCompletedEvent)e;
            return new WorkflowExecutionCompletedEventSchema(evt.AggregateId.Value, evt.ExecutionHash);
        });
        schema.RegisterPayloadMapper("WorkflowExecutionFailedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionFailedEvent)e;
            return new WorkflowExecutionFailedEventSchema(
                evt.AggregateId.Value, evt.FailedStepName, evt.Reason);
        });
        schema.RegisterPayloadMapper("WorkflowExecutionResumedEvent", e =>
        {
            var evt = (DomainEvents.WorkflowExecutionResumedEvent)e;
            return new WorkflowExecutionResumedEventSchema(
                evt.AggregateId.Value, evt.ResumedFromStepName, evt.PreviousFailureReason);
        });
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var handler = provider.GetRequiredService<WorkflowExecutionProjectionHandler>();
        projection.Register("WorkflowExecutionStartedEvent", handler);
        projection.Register("WorkflowStepCompletedEvent", handler);
        projection.Register("WorkflowExecutionCompletedEvent", handler);
        projection.Register("WorkflowExecutionFailedEvent", handler);
        projection.Register("WorkflowExecutionResumedEvent", handler);
    }

    public void RegisterEngines(IEngineRegistry engine) { }

    public void RegisterWorkflows(IWorkflowRegistry workflow) { }
}
