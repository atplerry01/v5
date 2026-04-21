using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Clause.Application;

public static class ClauseApplicationModule
{
    public static IServiceCollection AddClauseApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateClauseHandler>();
        services.AddTransient<ActivateClauseHandler>();
        services.AddTransient<SupersedeClauseHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateClauseCommand, CreateClauseHandler>();
        engine.Register<ActivateClauseCommand, ActivateClauseHandler>();
        engine.Register<SupersedeClauseCommand, SupersedeClauseHandler>();
    }
}
