using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Capital.Asset.Application;

public static class CapitalAssetApplicationModule
{
    public static IServiceCollection AddCapitalAssetApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAssetHandler>();
        services.AddTransient<RevalueAssetHandler>();
        services.AddTransient<DisposeAssetHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAssetCommand, CreateAssetHandler>();
        engine.Register<RevalueAssetCommand, RevalueAssetHandler>();
        engine.Register<DisposeAssetCommand, DisposeAssetHandler>();
    }
}
