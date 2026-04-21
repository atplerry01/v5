using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Service.ServiceCore.ServiceLevel.Application;

public static class ServiceLevelApplicationModule
{
    public static IServiceCollection AddServiceLevelApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateServiceLevelHandler>();
        services.AddTransient<UpdateServiceLevelHandler>();
        services.AddTransient<ActivateServiceLevelHandler>();
        services.AddTransient<ArchiveServiceLevelHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateServiceLevelCommand, CreateServiceLevelHandler>();
        engine.Register<UpdateServiceLevelCommand, UpdateServiceLevelHandler>();
        engine.Register<ActivateServiceLevelCommand, ActivateServiceLevelHandler>();
        engine.Register<ArchiveServiceLevelCommand, ArchiveServiceLevelHandler>();
    }
}
