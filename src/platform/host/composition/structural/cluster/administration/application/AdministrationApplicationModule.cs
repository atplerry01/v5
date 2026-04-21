using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Administration;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Administration.Application;

public static class AdministrationApplicationModule
{
    public static IServiceCollection AddAdministrationApplication(this IServiceCollection services)
    {
        services.AddTransient<EstablishAdministrationHandler>();
        services.AddTransient<EstablishAdministrationWithParentHandler>();
        services.AddTransient<ActivateAdministrationHandler>();
        services.AddTransient<SuspendAdministrationHandler>();
        services.AddTransient<RetireAdministrationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<EstablishAdministrationCommand, EstablishAdministrationHandler>();
        engine.Register<EstablishAdministrationWithParentCommand, EstablishAdministrationWithParentHandler>();
        engine.Register<ActivateAdministrationCommand, ActivateAdministrationHandler>();
        engine.Register<SuspendAdministrationCommand, SuspendAdministrationHandler>();
        engine.Register<RetireAdministrationCommand, RetireAdministrationHandler>();
    }
}
