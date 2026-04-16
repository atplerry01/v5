using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Contract;

/// <summary>
/// Contract composition module — T2E handler DI registrations
/// and engine registry binding for the 3 contract commands.
/// No T1M workflow: each command is single-aggregate, no orchestration needed.
/// </summary>
public static class ContractCompositionModule
{
    public static IServiceCollection AddContract(this IServiceCollection services)
    {
        services.AddTransient<CreateRevenueContractHandler>();
        services.AddTransient<ActivateRevenueContractHandler>();
        services.AddTransient<TerminateRevenueContractHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateRevenueContractCommand, CreateRevenueContractHandler>();
        engine.Register<ActivateRevenueContractCommand, ActivateRevenueContractHandler>();
        engine.Register<TerminateRevenueContractCommand, TerminateRevenueContractHandler>();
    }
}
