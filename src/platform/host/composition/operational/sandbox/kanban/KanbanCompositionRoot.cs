using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban.Application;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban.Infrastructure;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban.Projection;
using Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban.Workflow;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban;

/// <summary>
/// Composition root for operational/sandbox/kanban. Delegates to modular sub-units
/// for application, projection, workflow, and infrastructure concerns.
/// </summary>
public sealed class KanbanCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKanbanApplication();
        services.AddKanbanProjection(configuration);
        services.AddKanbanWorkflow();
        services.AddKanbanInfrastructure();
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterOperationalSandboxKanban(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        KanbanProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        KanbanApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        KanbanWorkflowModule.RegisterWorkflows(workflow);
    }
}
