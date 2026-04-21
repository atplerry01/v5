using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.PartyGovernance.Counterparty.Application;

public static class CounterpartyApplicationModule
{
    public static IServiceCollection AddCounterpartyApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCounterpartyHandler>();
        services.AddTransient<SuspendCounterpartyHandler>();
        services.AddTransient<TerminateCounterpartyHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateCounterpartyCommand, CreateCounterpartyHandler>();
        engine.Register<SuspendCounterpartyCommand, SuspendCounterpartyHandler>();
        engine.Register<TerminateCounterpartyCommand, TerminateCounterpartyHandler>();
    }
}
