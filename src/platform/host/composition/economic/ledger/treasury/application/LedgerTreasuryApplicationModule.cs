using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Ledger.Treasury.Application;

public static class LedgerTreasuryApplicationModule
{
    public static IServiceCollection AddLedgerTreasuryApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateTreasuryHandler>();
        services.AddTransient<AllocateFundsHandler>();
        services.AddTransient<ReleaseFundsHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateTreasuryCommand, CreateTreasuryHandler>();
        engine.Register<AllocateFundsCommand, AllocateFundsHandler>();
        engine.Register<ReleaseFundsCommand, ReleaseFundsHandler>();
    }
}
