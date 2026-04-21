using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Structure.TopologyDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;

namespace Whycespace.Platform.Host.Composition.Structural.Structure.TopologyDefinition.Application;

public static class TopologyDefinitionApplicationModule
{
    public static IServiceCollection AddTopologyDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateTopologyDefinitionHandler>();
        services.AddTransient<ActivateTopologyDefinitionHandler>();
        services.AddTransient<SuspendTopologyDefinitionHandler>();
        services.AddTransient<ReactivateTopologyDefinitionHandler>();
        services.AddTransient<RetireTopologyDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateTopologyDefinitionCommand, CreateTopologyDefinitionHandler>();
        engine.Register<ActivateTopologyDefinitionCommand, ActivateTopologyDefinitionHandler>();
        engine.Register<SuspendTopologyDefinitionCommand, SuspendTopologyDefinitionHandler>();
        engine.Register<ReactivateTopologyDefinitionCommand, ReactivateTopologyDefinitionHandler>();
        engine.Register<RetireTopologyDefinitionCommand, RetireTopologyDefinitionHandler>();
    }
}
