using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Composition.Operational.Sandbox.Todo.Application;
using Whyce.Platform.Host.Composition.Operational.Sandbox.Todo.Infrastructure;
using Whyce.Platform.Host.Composition.Operational.Sandbox.Todo.Projection;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.EventFabric.DomainSchemas;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Host.Composition.Operational.Sandbox.Todo;

/// <summary>
/// Composition root for operational/sandbox/todo. Delegates to modular sub-units
/// for application, projection, and infrastructure concerns.
/// No T1M workflows for Todo — all commands dispatch directly to T2E.
/// </summary>
public sealed class TodoCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTodoApplication();
        services.AddTodoProjection(configuration);
        services.AddTodoInfrastructure();
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterOperationalSandboxTodo(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        TodoProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        TodoApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows for Todo — all commands dispatch directly to T2E.
    }
}
