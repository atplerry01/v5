using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Pricing;

/// <summary>
/// Pricing composition module — T2E handler DI registrations
/// and engine registry binding for the 2 pricing commands.
/// No T1M workflow: each command is single-aggregate, no orchestration needed.
/// </summary>
public static class PricingCompositionModule
{
    public static IServiceCollection AddPricing(this IServiceCollection services)
    {
        services.AddTransient<DefinePricingHandler>();
        services.AddTransient<AdjustPricingHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefinePricingCommand, DefinePricingHandler>();
        engine.Register<AdjustPricingCommand, AdjustPricingHandler>();
    }
}
