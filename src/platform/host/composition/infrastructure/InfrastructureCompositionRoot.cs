using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Infrastructure.Authentication;
using Whycespace.Platform.Host.Composition.Infrastructure.Cache;
using Whycespace.Platform.Host.Composition.Infrastructure.Chain;
using Whycespace.Platform.Host.Composition.Infrastructure.Database;
using Whycespace.Platform.Host.Composition.Infrastructure.Messaging;
using Whycespace.Platform.Host.Composition.Infrastructure.Observability;
using Whycespace.Platform.Host.Composition.Infrastructure.Policy;

namespace Whycespace.Platform.Host.Composition.Infrastructure;

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
        // WP-1: Authentication FIRST — fail-closed before any other
        // infrastructure wiring. Missing signing key = startup halt.
        services.AddAuthentication(configuration);
        // R4.B / R-ADMIN-SCOPE-01: admin authorization policy layered on
        // top of the WP-1 authentication gate. Must run after
        // AddAuthentication so the ICallerIdentityAccessor is already
        // registered when the handler resolves.
        services.AddAdminAuthorization();
        services.AddDatabase(configuration);
        services.AddChain();
        services.AddCache(configuration);
        services.AddPolicy(configuration);
        services.AddMessaging(configuration);
        services.AddInfrastructureObservability();
        // R5.A / R-TRACE-EXPORTER-OTEL-01 — OpenTelemetry tracing bootstrap.
        // Must run AFTER observability infra (resource builder) and policy
        // (so operations with policy evaluation appear as child spans).
        services.AddTracing(configuration);
        return services;
    }
}
