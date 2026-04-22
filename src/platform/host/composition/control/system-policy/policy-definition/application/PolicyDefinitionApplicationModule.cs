using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyDefinition.Application;

public static class PolicyDefinitionApplicationModule
{
    public static IServiceCollection AddPolicyDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<DefinePolicyHandler>();
        services.AddTransient<DeprecatePolicyHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefinePolicyCommand, DefinePolicyHandler>();
        engine.Register<DeprecatePolicyCommand, DeprecatePolicyHandler>();
    }
}
