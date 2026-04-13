using Microsoft.Extensions.DependencyInjection;
using Whyce.Systems.Downstream.Operational.Sandbox.Kanban;

namespace Whyce.Platform.Host.Composition.Operational.Sandbox.Kanban.Infrastructure;

/// <summary>
/// Kanban infrastructure module — Systems.Downstream intent handler registration.
/// </summary>
public static class KanbanInfrastructureModule
{
    public static IServiceCollection AddKanbanInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IKanbanIntentHandler, KanbanIntentHandler>();
        return services;
    }
}
