using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Ledger.Obligation.Application;

public static class LedgerObligationApplicationModule
{
    public static IServiceCollection AddLedgerObligationApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateObligationHandler>();
        services.AddTransient<FulfilObligationHandler>();
        services.AddTransient<CancelObligationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateObligationCommand, CreateObligationHandler>();
        engine.Register<FulfilObligationCommand, FulfilObligationHandler>();
        engine.Register<CancelObligationCommand, CancelObligationHandler>();
    }
}
