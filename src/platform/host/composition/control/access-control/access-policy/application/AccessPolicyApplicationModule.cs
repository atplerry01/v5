using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.AccessControl.AccessPolicy.Application;

public static class AccessPolicyApplicationModule
{
    public static IServiceCollection AddAccessPolicyApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineAccessPolicyHandler>();
        services.AddTransient<ActivateAccessPolicyHandler>();
        services.AddTransient<RetireAccessPolicyHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineAccessPolicyCommand, DefineAccessPolicyHandler>();
        engine.Register<ActivateAccessPolicyCommand, ActivateAccessPolicyHandler>();
        engine.Register<RetireAccessPolicyCommand, RetireAccessPolicyHandler>();
    }
}
