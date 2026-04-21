using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.PlaybackConsumption.Session.Application;

public static class SessionApplicationModule
{
    public static IServiceCollection AddSessionApplication(this IServiceCollection services)
    {
        services.AddTransient<OpenSessionHandler>();
        services.AddTransient<ActivateSessionHandler>();
        services.AddTransient<SuspendSessionHandler>();
        services.AddTransient<ResumeSessionHandler>();
        services.AddTransient<CloseSessionHandler>();
        services.AddTransient<FailSessionHandler>();
        services.AddTransient<ExpireSessionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<OpenSessionCommand, OpenSessionHandler>();
        engine.Register<ActivateSessionCommand, ActivateSessionHandler>();
        engine.Register<SuspendSessionCommand, SuspendSessionHandler>();
        engine.Register<ResumeSessionCommand, ResumeSessionHandler>();
        engine.Register<CloseSessionCommand, CloseSessionHandler>();
        engine.Register<FailSessionCommand, FailSessionHandler>();
        engine.Register<ExpireSessionCommand, ExpireSessionHandler>();
    }
}
