using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Provider.ProviderCore.ProviderCapability.Application;

public static class ProviderCapabilityApplicationModule
{
    public static IServiceCollection AddProviderCapabilityApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProviderCapabilityHandler>();
        services.AddTransient<UpdateProviderCapabilityHandler>();
        services.AddTransient<ActivateProviderCapabilityHandler>();
        services.AddTransient<ArchiveProviderCapabilityHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProviderCapabilityCommand, CreateProviderCapabilityHandler>();
        engine.Register<UpdateProviderCapabilityCommand, UpdateProviderCapabilityHandler>();
        engine.Register<ActivateProviderCapabilityCommand, ActivateProviderCapabilityHandler>();
        engine.Register<ArchiveProviderCapabilityCommand, ArchiveProviderCapabilityHandler>();
    }
}
