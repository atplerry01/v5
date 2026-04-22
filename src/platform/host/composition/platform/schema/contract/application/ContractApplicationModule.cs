using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Schema.Contract;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Schema.Contract.Application;

public static class ContractApplicationModule
{
    public static IServiceCollection AddContractApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterContractHandler>();
        services.AddTransient<AddContractSubscriberHandler>();
        services.AddTransient<DeprecateContractHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterContractCommand, RegisterContractHandler>();
        engine.Register<AddContractSubscriberCommand, AddContractSubscriberHandler>();
        engine.Register<DeprecateContractCommand, DeprecateContractHandler>();
    }
}
