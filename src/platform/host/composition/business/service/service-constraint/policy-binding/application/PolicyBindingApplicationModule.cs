using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Service.ServiceConstraint.PolicyBinding.Application;

public static class PolicyBindingApplicationModule
{
    public static IServiceCollection AddPolicyBindingApplication(this IServiceCollection services)
    {
        services.AddTransient<CreatePolicyBindingHandler>();
        services.AddTransient<BindPolicyBindingHandler>();
        services.AddTransient<UnbindPolicyBindingHandler>();
        services.AddTransient<ArchivePolicyBindingHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreatePolicyBindingCommand, CreatePolicyBindingHandler>();
        engine.Register<BindPolicyBindingCommand, BindPolicyBindingHandler>();
        engine.Register<UnbindPolicyBindingCommand, UnbindPolicyBindingHandler>();
        engine.Register<ArchivePolicyBindingCommand, ArchivePolicyBindingHandler>();
    }
}
