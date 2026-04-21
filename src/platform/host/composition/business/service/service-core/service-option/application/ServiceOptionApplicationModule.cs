using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Service.ServiceCore.ServiceOption.Application;

public static class ServiceOptionApplicationModule
{
    public static IServiceCollection AddServiceOptionApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateServiceOptionHandler>();
        services.AddTransient<UpdateServiceOptionHandler>();
        services.AddTransient<ActivateServiceOptionHandler>();
        services.AddTransient<ArchiveServiceOptionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateServiceOptionCommand, CreateServiceOptionHandler>();
        engine.Register<UpdateServiceOptionCommand, UpdateServiceOptionHandler>();
        engine.Register<ActivateServiceOptionCommand, ActivateServiceOptionHandler>();
        engine.Register<ArchiveServiceOptionCommand, ArchiveServiceOptionHandler>();
    }
}
