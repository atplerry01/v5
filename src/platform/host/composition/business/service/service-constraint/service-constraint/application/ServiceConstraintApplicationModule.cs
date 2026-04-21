using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Service.ServiceConstraint.ServiceConstraint.Application;

public static class ServiceConstraintApplicationModule
{
    public static IServiceCollection AddServiceConstraintApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateServiceConstraintHandler>();
        services.AddTransient<UpdateServiceConstraintHandler>();
        services.AddTransient<ActivateServiceConstraintHandler>();
        services.AddTransient<ArchiveServiceConstraintHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateServiceConstraintCommand, CreateServiceConstraintHandler>();
        engine.Register<UpdateServiceConstraintCommand, UpdateServiceConstraintHandler>();
        engine.Register<ActivateServiceConstraintCommand, ActivateServiceConstraintHandler>();
        engine.Register<ArchiveServiceConstraintCommand, ArchiveServiceConstraintHandler>();
    }
}
