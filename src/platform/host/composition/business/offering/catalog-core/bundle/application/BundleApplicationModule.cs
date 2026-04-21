using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Offering.CatalogCore.Bundle.Application;

public static class BundleApplicationModule
{
    public static IServiceCollection AddBundleApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateBundleHandler>();
        services.AddTransient<AddBundleMemberHandler>();
        services.AddTransient<RemoveBundleMemberHandler>();
        services.AddTransient<ActivateBundleHandler>();
        services.AddTransient<ArchiveBundleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateBundleCommand, CreateBundleHandler>();
        engine.Register<AddBundleMemberCommand, AddBundleMemberHandler>();
        engine.Register<RemoveBundleMemberCommand, RemoveBundleMemberHandler>();
        engine.Register<ActivateBundleCommand, ActivateBundleHandler>();
        engine.Register<ArchiveBundleCommand, ArchiveBundleHandler>();
    }
}
