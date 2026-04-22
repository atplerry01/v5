using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Observability.SystemSignal.Application;

public static class SystemSignalApplicationModule
{
    public static IServiceCollection AddSystemSignalApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineSystemSignalHandler>();
        services.AddTransient<DeprecateSystemSignalHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineSystemSignalCommand, DefineSystemSignalHandler>();
        engine.Register<DeprecateSystemSignalCommand, DeprecateSystemSignalHandler>();
    }
}
