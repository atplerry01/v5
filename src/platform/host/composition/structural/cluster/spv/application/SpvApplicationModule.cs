using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Spv;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Spv.Application;

public static class SpvApplicationModule
{
    public static IServiceCollection AddSpvApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateSpvHandler>();
        services.AddTransient<CreateSpvWithParentHandler>();
        services.AddTransient<ActivateSpvHandler>();
        services.AddTransient<SuspendSpvHandler>();
        services.AddTransient<CloseSpvHandler>();
        services.AddTransient<ReactivateSpvHandler>();
        services.AddTransient<RetireSpvHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateSpvCommand, CreateSpvHandler>();
        engine.Register<CreateSpvWithParentCommand, CreateSpvWithParentHandler>();
        engine.Register<ActivateSpvCommand, ActivateSpvHandler>();
        engine.Register<SuspendSpvCommand, SuspendSpvHandler>();
        engine.Register<CloseSpvCommand, CloseSpvHandler>();
        engine.Register<ReactivateSpvCommand, ReactivateSpvHandler>();
        engine.Register<RetireSpvCommand, RetireSpvHandler>();
    }
}
