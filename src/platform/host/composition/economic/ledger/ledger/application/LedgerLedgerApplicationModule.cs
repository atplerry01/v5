using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Ledger.Ledger.Application;

public static class LedgerLedgerApplicationModule
{
    public static IServiceCollection AddLedgerLedgerApplication(this IServiceCollection services)
    {
        services.AddTransient<OpenLedgerHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<OpenLedgerCommand, OpenLedgerHandler>();
    }
}
