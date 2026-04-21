using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Renewal.Application;

public static class RenewalApplicationModule
{
    public static IServiceCollection AddRenewalApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateRenewalHandler>();
        services.AddTransient<RenewRenewalHandler>();
        services.AddTransient<ExpireRenewalHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateRenewalCommand, CreateRenewalHandler>();
        engine.Register<RenewRenewalCommand, RenewRenewalHandler>();
        engine.Register<ExpireRenewalCommand, ExpireRenewalHandler>();
    }
}
