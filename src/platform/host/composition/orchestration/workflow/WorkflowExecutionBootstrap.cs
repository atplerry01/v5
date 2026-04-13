using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Orchestration.Workflow;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Projections.Orchestration.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Orchestration.Workflow;

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
        // Phase 1.5 §5.1.2 BPV-D01: schema identity binding lives in the
        // runtime-side WorkflowExecutionSchemaModule. Host stays free of
        // typed domain refs.
        DomainSchemaCatalog.RegisterOrchestrationWorkflowExecution(schema);
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
