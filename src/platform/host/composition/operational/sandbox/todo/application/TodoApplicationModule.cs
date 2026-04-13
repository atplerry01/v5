using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;

namespace Whycespace.Platform.Host.Composition.Operational.Sandbox.Todo.Application;

/// <summary>
/// Todo application module — T2E command handler DI registrations and engine bindings.
/// </summary>
public static class TodoApplicationModule
{
    public static IServiceCollection AddTodoApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateTodoHandler>();
        services.AddTransient<UpdateTodoHandler>();
        services.AddTransient<CompleteTodoHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateTodoCommand, CreateTodoHandler>();
        engine.Register<UpdateTodoCommand, UpdateTodoHandler>();
        engine.Register<CompleteTodoCommand, CompleteTodoHandler>();
    }
}
