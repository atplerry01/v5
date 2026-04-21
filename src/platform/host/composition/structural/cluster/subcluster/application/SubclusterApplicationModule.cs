using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Subcluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Subcluster.Application;

public static class SubclusterApplicationModule
{
    public static IServiceCollection AddSubclusterApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineSubclusterHandler>();
        services.AddTransient<DefineSubclusterWithParentHandler>();
        services.AddTransient<ActivateSubclusterHandler>();
        services.AddTransient<SuspendSubclusterHandler>();
        services.AddTransient<ReactivateSubclusterHandler>();
        services.AddTransient<ArchiveSubclusterHandler>();
        services.AddTransient<RetireSubclusterHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineSubclusterCommand, DefineSubclusterHandler>();
        engine.Register<DefineSubclusterWithParentCommand, DefineSubclusterWithParentHandler>();
        engine.Register<ActivateSubclusterCommand, ActivateSubclusterHandler>();
        engine.Register<SuspendSubclusterCommand, SuspendSubclusterHandler>();
        engine.Register<ReactivateSubclusterCommand, ReactivateSubclusterHandler>();
        engine.Register<ArchiveSubclusterCommand, ArchiveSubclusterHandler>();
        engine.Register<RetireSubclusterCommand, RetireSubclusterHandler>();
    }
}
