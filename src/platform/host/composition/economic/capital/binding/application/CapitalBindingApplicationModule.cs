using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Capital.Binding.Application;

public static class CapitalBindingApplicationModule
{
    public static IServiceCollection AddCapitalBindingApplication(this IServiceCollection services)
    {
        services.AddTransient<BindCapitalHandler>();
        services.AddTransient<TransferBindingOwnershipHandler>();
        services.AddTransient<ReleaseBindingHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<BindCapitalCommand, BindCapitalHandler>();
        engine.Register<TransferBindingOwnershipCommand, TransferBindingOwnershipHandler>();
        engine.Register<ReleaseBindingCommand, ReleaseBindingHandler>();
    }
}
