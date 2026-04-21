using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Contract.Application;

public static class ContractApplicationModule
{
    public static IServiceCollection AddContractApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateContractHandler>();
        services.AddTransient<AddPartyToContractHandler>();
        services.AddTransient<ActivateContractHandler>();
        services.AddTransient<SuspendContractHandler>();
        services.AddTransient<TerminateContractHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateContractCommand, CreateContractHandler>();
        engine.Register<AddPartyToContractCommand, AddPartyToContractHandler>();
        engine.Register<ActivateContractCommand, ActivateContractHandler>();
        engine.Register<SuspendContractCommand, SuspendContractHandler>();
        engine.Register<TerminateContractCommand, TerminateContractHandler>();
    }
}
