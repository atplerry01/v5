using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.LifecycleChange.Version.Application;

public static class MediaVersionApplicationModule
{
    public static IServiceCollection AddMediaVersionApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateMediaVersionHandler>();
        services.AddTransient<ActivateMediaVersionHandler>();
        services.AddTransient<SupersedeMediaVersionHandler>();
        services.AddTransient<WithdrawMediaVersionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateMediaVersionCommand, CreateMediaVersionHandler>();
        engine.Register<ActivateMediaVersionCommand, ActivateMediaVersionHandler>();
        engine.Register<SupersedeMediaVersionCommand, SupersedeMediaVersionHandler>();
        engine.Register<WithdrawMediaVersionCommand, WithdrawMediaVersionHandler>();
    }
}
