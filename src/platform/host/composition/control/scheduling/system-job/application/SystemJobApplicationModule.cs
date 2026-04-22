using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Scheduling.SystemJob.Application;

public static class SystemJobApplicationModule
{
    public static IServiceCollection AddSystemJobApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineSystemJobHandler>();
        services.AddTransient<DeprecateSystemJobHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineSystemJobCommand, DefineSystemJobHandler>();
        engine.Register<DeprecateSystemJobCommand, DeprecateSystemJobHandler>();
    }
}
