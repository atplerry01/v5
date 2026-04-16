using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Revenue.Application;

public static class RevenueRevenueApplicationModule
{
    public static IServiceCollection AddRevenueRevenueApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordRevenueHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordRevenueCommand, RecordRevenueHandler>();
    }
}
