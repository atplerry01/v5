using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Capital.Reserve.Application;

public static class CapitalReserveApplicationModule
{
    public static IServiceCollection AddCapitalReserveApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCapitalReserveHandler>();
        services.AddTransient<ReleaseCapitalReserveHandler>();
        services.AddTransient<ExpireCapitalReserveHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateCapitalReserveCommand, CreateCapitalReserveHandler>();
        engine.Register<ReleaseCapitalReserveCommand, ReleaseCapitalReserveHandler>();
        engine.Register<ExpireCapitalReserveCommand, ExpireCapitalReserveHandler>();
    }
}
