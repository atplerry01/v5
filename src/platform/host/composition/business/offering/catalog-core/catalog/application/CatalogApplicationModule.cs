using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.Catalog.Application;

public static class CatalogApplicationModule
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCatalogHandler>();
        services.AddTransient<AddCatalogMemberHandler>();
        services.AddTransient<RemoveCatalogMemberHandler>();
        services.AddTransient<PublishCatalogHandler>();
        services.AddTransient<ArchiveCatalogHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateCatalogCommand, CreateCatalogHandler>();
        engine.Register<AddCatalogMemberCommand, AddCatalogMemberHandler>();
        engine.Register<RemoveCatalogMemberCommand, RemoveCatalogMemberHandler>();
        engine.Register<PublishCatalogCommand, PublishCatalogHandler>();
        engine.Register<ArchiveCatalogCommand, ArchiveCatalogHandler>();
    }
}
