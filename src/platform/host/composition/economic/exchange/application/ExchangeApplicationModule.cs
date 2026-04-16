using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Exchange.Fx;
using Whycespace.Engines.T2E.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Exchange.Application;

public static class ExchangeApplicationModule
{
    public static IServiceCollection AddExchangeApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterFxPairHandler>();
        services.AddTransient<ActivateFxPairHandler>();
        services.AddTransient<DeactivateFxPairHandler>();
        services.AddTransient<DefineExchangeRateHandler>();
        services.AddTransient<ActivateExchangeRateHandler>();
        services.AddTransient<ExpireExchangeRateHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterFxPairCommand, RegisterFxPairHandler>();
        engine.Register<ActivateFxPairCommand, ActivateFxPairHandler>();
        engine.Register<DeactivateFxPairCommand, DeactivateFxPairHandler>();
        engine.Register<DefineExchangeRateCommand, DefineExchangeRateHandler>();
        engine.Register<ActivateExchangeRateCommand, ActivateExchangeRateHandler>();
        engine.Register<ExpireExchangeRateCommand, ExpireExchangeRateHandler>();
    }
}
