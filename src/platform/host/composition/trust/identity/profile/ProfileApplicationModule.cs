using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Trust.Identity.Profile;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;

namespace Whycespace.Platform.Host.Composition.Trust.Identity.Profile;

public static class ProfileApplicationModule
{
    public static IServiceCollection AddProfileApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProfileHandler>();
        services.AddTransient<ActivateProfileHandler>();
        services.AddTransient<DeactivateProfileHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProfileCommand, CreateProfileHandler>();
        engine.Register<ActivateProfileCommand, ActivateProfileHandler>();
        engine.Register<DeactivateProfileCommand, DeactivateProfileHandler>();
    }
}
