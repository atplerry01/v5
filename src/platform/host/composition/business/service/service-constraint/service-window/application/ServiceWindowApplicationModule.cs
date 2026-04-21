using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Service.ServiceConstraint.ServiceWindow.Application;

public static class ServiceWindowApplicationModule
{
    public static IServiceCollection AddServiceWindowApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateServiceWindowHandler>();
        services.AddTransient<UpdateServiceWindowHandler>();
        services.AddTransient<ActivateServiceWindowHandler>();
        services.AddTransient<ArchiveServiceWindowHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateServiceWindowCommand, CreateServiceWindowHandler>();
        engine.Register<UpdateServiceWindowCommand, UpdateServiceWindowHandler>();
        engine.Register<ActivateServiceWindowCommand, ActivateServiceWindowHandler>();
        engine.Register<ArchiveServiceWindowCommand, ArchiveServiceWindowHandler>();
    }
}
