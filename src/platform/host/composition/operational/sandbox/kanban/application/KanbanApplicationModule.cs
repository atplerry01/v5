using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Operational.Sandbox.Kanban;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.List;

namespace Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban.Application;

/// <summary>
/// Kanban application module — T2E command handler DI registrations and engine bindings.
/// </summary>
public static class KanbanApplicationModule
{
    public static IServiceCollection AddKanbanApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateKanbanBoardHandler>();
        services.AddTransient<CreateKanbanListHandler>();
        services.AddTransient<CreateKanbanCardHandler>();
        services.AddTransient<MoveKanbanCardHandler>();
        services.AddTransient<ReorderKanbanCardHandler>();
        services.AddTransient<CompleteKanbanCardHandler>();
        services.AddTransient<UpdateKanbanCardHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateKanbanBoardCommand, CreateKanbanBoardHandler>();
        engine.Register<CreateKanbanListCommand, CreateKanbanListHandler>();
        engine.Register<CreateKanbanCardCommand, CreateKanbanCardHandler>();
        engine.Register<MoveKanbanCardCommand, MoveKanbanCardHandler>();
        engine.Register<ReorderKanbanCardCommand, ReorderKanbanCardHandler>();
        engine.Register<CompleteKanbanCardCommand, CompleteKanbanCardHandler>();
        engine.Register<UpdateKanbanCardCommand, UpdateKanbanCardHandler>();
    }
}
