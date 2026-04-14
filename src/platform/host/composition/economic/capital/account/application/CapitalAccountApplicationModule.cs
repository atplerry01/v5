using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Capital.Account.Application;

public static class CapitalAccountApplicationModule
{
    public static IServiceCollection AddCapitalAccountApplication(this IServiceCollection services)
    {
        services.AddTransient<OpenCapitalAccountHandler>();
        services.AddTransient<FundCapitalAccountHandler>();
        services.AddTransient<AllocateCapitalAccountHandler>();
        services.AddTransient<ReserveCapitalAccountHandler>();
        services.AddTransient<ReleaseCapitalReservationHandler>();
        services.AddTransient<FreezeCapitalAccountHandler>();
        services.AddTransient<CloseCapitalAccountHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<OpenCapitalAccountCommand, OpenCapitalAccountHandler>();
        engine.Register<FundCapitalAccountCommand, FundCapitalAccountHandler>();
        engine.Register<AllocateCapitalAccountCommand, AllocateCapitalAccountHandler>();
        engine.Register<ReserveCapitalAccountCommand, ReserveCapitalAccountHandler>();
        engine.Register<ReleaseCapitalReservationCommand, ReleaseCapitalReservationHandler>();
        engine.Register<FreezeCapitalAccountCommand, FreezeCapitalAccountHandler>();
        engine.Register<CloseCapitalAccountCommand, CloseCapitalAccountHandler>();
    }
}
