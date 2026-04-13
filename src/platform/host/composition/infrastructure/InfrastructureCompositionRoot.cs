using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Composition.Infrastructure.Cache;
using Whyce.Platform.Host.Composition.Infrastructure.Chain;
using Whyce.Platform.Host.Composition.Infrastructure.Database;
using Whyce.Platform.Host.Composition.Infrastructure.Messaging;
using Whyce.Platform.Host.Composition.Infrastructure.Observability;
using Whyce.Platform.Host.Composition.Infrastructure.Policy;

namespace Whyce.Platform.Host.Composition.Infrastructure;

/// <summary>
/// Infrastructure composition root — delegates to capability-based sub-modules.
/// Registration order matters: database first (data sources), then capabilities
/// that depend on those data sources (chain, messaging), then independent
/// capabilities (cache, policy, observability).
/// </summary>
public static class InfrastructureCompositionRoot
{
    public static IServiceCollection AddInfrastructureComposition(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddChain();
        services.AddCache(configuration);
        services.AddPolicy(configuration);
        services.AddMessaging(configuration);
        services.AddInfrastructureObservability();
        return services;
    }
}
