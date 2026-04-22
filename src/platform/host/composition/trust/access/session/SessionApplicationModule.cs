using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Trust.Access.Session;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Access.Session;

namespace Whycespace.Platform.Host.Composition.Trust.Access.Session;

public static class SessionApplicationModule
{
    public static IServiceCollection AddSessionApplication(this IServiceCollection services)
    {
        services.AddTransient<OpenSessionHandler>();
        services.AddTransient<ExpireSessionHandler>();
        services.AddTransient<TerminateSessionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<OpenSessionCommand, OpenSessionHandler>();
        engine.Register<ExpireSessionCommand, ExpireSessionHandler>();
        engine.Register<TerminateSessionCommand, TerminateSessionHandler>();
    }
}
