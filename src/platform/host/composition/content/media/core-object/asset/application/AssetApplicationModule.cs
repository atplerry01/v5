using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.CoreObject.Asset.Application;

public static class AssetApplicationModule
{
    public static IServiceCollection AddAssetApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAssetHandler>();
        services.AddTransient<RenameAssetHandler>();
        services.AddTransient<ReclassifyAssetHandler>();
        services.AddTransient<ActivateAssetHandler>();
        services.AddTransient<RetireAssetHandler>();
        services.AddTransient<AssignAssetKindHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAssetCommand, CreateAssetHandler>();
        engine.Register<RenameAssetCommand, RenameAssetHandler>();
        engine.Register<ReclassifyAssetCommand, ReclassifyAssetHandler>();
        engine.Register<ActivateAssetCommand, ActivateAssetHandler>();
        engine.Register<RetireAssetCommand, RetireAssetHandler>();
        engine.Register<AssignAssetKindCommand, AssignAssetKindHandler>();
    }
}
