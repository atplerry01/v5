using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Topology;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Topology.Application;

public static class TopologyApplicationModule
{
    public static IServiceCollection AddTopologyApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineTopologyHandler>();
        services.AddTransient<ValidateTopologyHandler>();
        services.AddTransient<LockTopologyHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineTopologyCommand, DefineTopologyHandler>();
        engine.Register<ValidateTopologyCommand, ValidateTopologyHandler>();
        engine.Register<LockTopologyCommand, LockTopologyHandler>();
    }
}
