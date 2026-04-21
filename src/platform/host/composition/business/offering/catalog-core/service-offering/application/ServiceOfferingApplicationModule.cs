using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.ServiceOffering.Application;

public static class ServiceOfferingApplicationModule
{
    public static IServiceCollection AddServiceOfferingApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateServiceOfferingHandler>();
        services.AddTransient<UpdateServiceOfferingHandler>();
        services.AddTransient<ActivateServiceOfferingHandler>();
        services.AddTransient<ArchiveServiceOfferingHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateServiceOfferingCommand, CreateServiceOfferingHandler>();
        engine.Register<UpdateServiceOfferingCommand, UpdateServiceOfferingHandler>();
        engine.Register<ActivateServiceOfferingCommand, ActivateServiceOfferingHandler>();
        engine.Register<ArchiveServiceOfferingCommand, ArchiveServiceOfferingHandler>();
    }
}
