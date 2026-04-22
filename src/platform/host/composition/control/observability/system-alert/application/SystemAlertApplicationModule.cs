using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Observability.SystemAlert.Application;

public static class SystemAlertApplicationModule
{
    public static IServiceCollection AddSystemAlertApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineSystemAlertHandler>();
        services.AddTransient<RetireSystemAlertHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineSystemAlertCommand, DefineSystemAlertHandler>();
        engine.Register<RetireSystemAlertCommand, RetireSystemAlertHandler>();
    }
}
