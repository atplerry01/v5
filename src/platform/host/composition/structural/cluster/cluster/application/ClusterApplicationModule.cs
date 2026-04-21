using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Cluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Cluster.Application;

public static class ClusterApplicationModule
{
    public static IServiceCollection AddClusterApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineClusterHandler>();
        services.AddTransient<ActivateClusterHandler>();
        services.AddTransient<ArchiveClusterHandler>();
        services.AddTransient<BindAuthorityToClusterHandler>();
        services.AddTransient<ReleaseAuthorityFromClusterHandler>();
        services.AddTransient<BindAdministrationToClusterHandler>();
        services.AddTransient<ReleaseAdministrationFromClusterHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineClusterCommand, DefineClusterHandler>();
        engine.Register<ActivateClusterCommand, ActivateClusterHandler>();
        engine.Register<ArchiveClusterCommand, ArchiveClusterHandler>();
        engine.Register<BindAuthorityToClusterCommand, BindAuthorityToClusterHandler>();
        engine.Register<ReleaseAuthorityFromClusterCommand, ReleaseAuthorityFromClusterHandler>();
        engine.Register<BindAdministrationToClusterCommand, BindAdministrationToClusterHandler>();
        engine.Register<ReleaseAdministrationFromClusterCommand, ReleaseAdministrationFromClusterHandler>();
    }
}
