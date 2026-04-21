using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Customer.IdentityAndProfile.Profile.Application;

public static class ProfileApplicationModule
{
    public static IServiceCollection AddProfileApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProfileHandler>();
        services.AddTransient<RenameProfileHandler>();
        services.AddTransient<SetProfileDescriptorHandler>();
        services.AddTransient<RemoveProfileDescriptorHandler>();
        services.AddTransient<ActivateProfileHandler>();
        services.AddTransient<ArchiveProfileHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProfileCommand, CreateProfileHandler>();
        engine.Register<RenameProfileCommand, RenameProfileHandler>();
        engine.Register<SetProfileDescriptorCommand, SetProfileDescriptorHandler>();
        engine.Register<RemoveProfileDescriptorCommand, RemoveProfileDescriptorHandler>();
        engine.Register<ActivateProfileCommand, ActivateProfileHandler>();
        engine.Register<ArchiveProfileCommand, ArchiveProfileHandler>();
    }
}
