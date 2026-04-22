using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemReconciliation.ConsistencyCheck.Application;

public static class ConsistencyCheckApplicationModule
{
    public static IServiceCollection AddConsistencyCheckApplication(this IServiceCollection services)
    {
        services.AddTransient<InitiateConsistencyCheckHandler>();
        services.AddTransient<CompleteConsistencyCheckHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<InitiateConsistencyCheckCommand, InitiateConsistencyCheckHandler>();
        engine.Register<CompleteConsistencyCheckCommand, CompleteConsistencyCheckHandler>();
    }
}
