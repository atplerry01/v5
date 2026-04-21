using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Service.ServiceCore.ServiceDefinition.Application;

public static class ServiceDefinitionApplicationModule
{
    public static IServiceCollection AddServiceDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateServiceDefinitionHandler>();
        services.AddTransient<UpdateServiceDefinitionHandler>();
        services.AddTransient<ActivateServiceDefinitionHandler>();
        services.AddTransient<ArchiveServiceDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateServiceDefinitionCommand, CreateServiceDefinitionHandler>();
        engine.Register<UpdateServiceDefinitionCommand, UpdateServiceDefinitionHandler>();
        engine.Register<ActivateServiceDefinitionCommand, ActivateServiceDefinitionHandler>();
        engine.Register<ArchiveServiceDefinitionCommand, ArchiveServiceDefinitionHandler>();
    }
}
