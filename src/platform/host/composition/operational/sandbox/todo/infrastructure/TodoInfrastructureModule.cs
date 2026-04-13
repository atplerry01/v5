using Microsoft.Extensions.DependencyInjection;
using Whycespace.Systems.Downstream.Operational.Sandbox.Todo;

namespace Whycespace.Platform.Host.Composition.Operational.Sandbox.Todo.Infrastructure;

/// <summary>
/// Todo infrastructure module — Systems.Downstream intent handler registration.
/// </summary>
public static class TodoInfrastructureModule
{
    public static IServiceCollection AddTodoInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<ITodoIntentHandler, TodoIntentHandler>();
        return services;
    }
}
