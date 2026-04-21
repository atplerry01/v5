using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Authority;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Authority.Application;

public static class AuthorityApplicationModule
{
    public static IServiceCollection AddAuthorityApplication(this IServiceCollection services)
    {
        services.AddTransient<EstablishAuthorityHandler>();
        services.AddTransient<EstablishAuthorityWithParentHandler>();
        services.AddTransient<ActivateAuthorityHandler>();
        services.AddTransient<RevokeAuthorityHandler>();
        services.AddTransient<SuspendAuthorityHandler>();
        services.AddTransient<ReactivateAuthorityHandler>();
        services.AddTransient<RetireAuthorityHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<EstablishAuthorityCommand, EstablishAuthorityHandler>();
        engine.Register<EstablishAuthorityWithParentCommand, EstablishAuthorityWithParentHandler>();
        engine.Register<ActivateAuthorityCommand, ActivateAuthorityHandler>();
        engine.Register<RevokeAuthorityCommand, RevokeAuthorityHandler>();
        engine.Register<SuspendAuthorityCommand, SuspendAuthorityHandler>();
        engine.Register<ReactivateAuthorityCommand, ReactivateAuthorityHandler>();
        engine.Register<RetireAuthorityCommand, RetireAuthorityHandler>();
    }
}
