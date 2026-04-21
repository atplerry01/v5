using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Obligation.Application;

public static class ObligationApplicationModule
{
    public static IServiceCollection AddObligationApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateObligationHandler>();
        services.AddTransient<FulfillObligationHandler>();
        services.AddTransient<BreachObligationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateObligationCommand, CreateObligationHandler>();
        engine.Register<FulfillObligationCommand, FulfillObligationHandler>();
        engine.Register<BreachObligationCommand, BreachObligationHandler>();
    }
}
