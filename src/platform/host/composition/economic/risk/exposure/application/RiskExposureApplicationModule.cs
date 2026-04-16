using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Risk.Exposure.Application;

public static class RiskExposureApplicationModule
{
    public static IServiceCollection AddRiskExposureApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateRiskExposureHandler>();
        services.AddTransient<IncreaseRiskExposureHandler>();
        services.AddTransient<ReduceRiskExposureHandler>();
        services.AddTransient<CloseRiskExposureHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateRiskExposureCommand, CreateRiskExposureHandler>();
        engine.Register<IncreaseRiskExposureCommand, IncreaseRiskExposureHandler>();
        engine.Register<ReduceRiskExposureCommand, ReduceRiskExposureHandler>();
        engine.Register<CloseRiskExposureCommand, CloseRiskExposureHandler>();
    }
}
