using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Provider.ProviderScope.ProviderAvailability.Application;

public static class ProviderAvailabilityApplicationModule
{
    public static IServiceCollection AddProviderAvailabilityApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProviderAvailabilityHandler>();
        services.AddTransient<UpdateProviderAvailabilityWindowHandler>();
        services.AddTransient<ActivateProviderAvailabilityHandler>();
        services.AddTransient<ArchiveProviderAvailabilityHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProviderAvailabilityCommand, CreateProviderAvailabilityHandler>();
        engine.Register<UpdateProviderAvailabilityWindowCommand, UpdateProviderAvailabilityWindowHandler>();
        engine.Register<ActivateProviderAvailabilityCommand, ActivateProviderAvailabilityHandler>();
        engine.Register<ArchiveProviderAvailabilityCommand, ArchiveProviderAvailabilityHandler>();
    }
}
