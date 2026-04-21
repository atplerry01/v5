using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.Product.Application;

public static class ProductApplicationModule
{
    public static IServiceCollection AddProductApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProductHandler>();
        services.AddTransient<UpdateProductHandler>();
        services.AddTransient<ActivateProductHandler>();
        services.AddTransient<ArchiveProductHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProductCommand, CreateProductHandler>();
        engine.Register<UpdateProductCommand, UpdateProductHandler>();
        engine.Register<ActivateProductCommand, ActivateProductHandler>();
        engine.Register<ArchiveProductCommand, ArchiveProductHandler>();
    }
}
