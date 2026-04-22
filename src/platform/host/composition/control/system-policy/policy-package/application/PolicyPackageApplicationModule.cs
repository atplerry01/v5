using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyPackage.Application;

public static class PolicyPackageApplicationModule
{
    public static IServiceCollection AddPolicyPackageApplication(this IServiceCollection services)
    {
        services.AddTransient<AssemblePolicyPackageHandler>();
        services.AddTransient<DeployPolicyPackageHandler>();
        services.AddTransient<RetirePolicyPackageHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<AssemblePolicyPackageCommand, AssemblePolicyPackageHandler>();
        engine.Register<DeployPolicyPackageCommand, DeployPolicyPackageHandler>();
        engine.Register<RetirePolicyPackageCommand, RetirePolicyPackageHandler>();
    }
}
