using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Provider.ProviderCore.ProviderTier.Application;

public static class ProviderTierApplicationModule
{
    public static IServiceCollection AddProviderTierApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProviderTierHandler>();
        services.AddTransient<UpdateProviderTierHandler>();
        services.AddTransient<ActivateProviderTierHandler>();
        services.AddTransient<ArchiveProviderTierHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProviderTierCommand, CreateProviderTierHandler>();
        engine.Register<UpdateProviderTierCommand, UpdateProviderTierHandler>();
        engine.Register<ActivateProviderTierCommand, ActivateProviderTierHandler>();
        engine.Register<ArchiveProviderTierCommand, ArchiveProviderTierHandler>();
    }
}
